using Microsoft.AspNetCore.Builder;
using TimeId.Operations;
using TimeId.Services;
using TimeId.DTOs.Geolocation;

var builder = WebApplication.CreateBuilder();
var path = Path.Combine(Path.GetTempPath(), "timeid-geo-" + Guid.NewGuid() + ".json");
builder.Configuration["TIMEID_DATA_PATH"] = path;
try
{
    var store = new DataStore(builder.Environment, builder.Configuration);
    var admin = store.Read(db => db.Accounts.First(x => x.Role == "Administrador").Id);
    var user = store.Read(db => db.Accounts.First(x => x.Role == "Usuário").Id);
    var team = new Team { Name = "Teste" };
    var staff = new Staff { Name = "Teste", DepartmentId = team.Id, Active = true };
    store.Write(db => { db.Departments.Add(team); db.Employees.Add(staff); db.Accounts.First(x => x.Id == user).EmployeeId = staff.Id; return true; });
    var service = new WorkLocationService(store);
    var validation = new GeolocationValidationService(store);
    void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("OK " + label); }
    void Reject(Action action, int status, string label) { try { action(); } catch (DomainError e) when (e.Status == status) { Check(true, label); return; } throw new Exception(label); }
    var input = new CreateWorkLocationRequest { Name = "Escritório", Latitude = 0, Longitude = 0, RadiusMeters = 100, DepartmentIds = new() { team.Id } };
    var location = service.Save(admin, null, input);
    var position = new PunchLocationInput { Latitude = 0, Longitude = 0, AccuracyMeters = 5 };
    Check(validation.Validate(user, staff.Id, position).Status == "DentroDaArea", "permissão por departamento");
    position.Longitude = 0.001; position.AccuracyMeters = 20;
    Check(validation.Validate(user, staff.Id, position).Status == "Inconclusivo", "incerteza na borda");
    position.AccuracyMeters = 1;
    Check(validation.Validate(user, staff.Id, position).Status == "ForaDaArea", "fora da área");
    Check(Math.Abs(GeolocationValidationService.DistanceMeters(0, 0, 0, 1) - 111195) < 2, "distância de referência");
    Reject(() => service.Save(user, null, input), 403, "usuário não gerencia locais");
    Reject(() => validation.Validate(user, Guid.NewGuid(), position), 403, "usuário não valida terceiros");
    input.Latitude = 91;
    Reject(() => service.Save(admin, null, input), 400, "latitude inválida"); input.Latitude = 0;
    input.DepartmentIds = new() { Guid.NewGuid() };
    Reject(() => service.Save(admin, null, input), 400, "vínculo inexistente"); input.DepartmentIds.Clear();
    input.EmployeeIds = new() { staff.Id }; service.Save(admin, location.Id, input);
    position.Longitude = 0;
    Check(validation.Validate(user, staff.Id, position).Status == "DentroDaArea", "permissão individual");
    input.EmployeeIds.Clear(); input.CompanyWide = true; service.Save(admin, location.Id, input);
    Check(validation.Validate(user, staff.Id, position).Status == "DentroDaArea", "permissão para empresa");
    input.CompanyWide = false;
    Reject(() => service.Save(admin, null, input), 400, "abrangência obrigatória");
    var restored = new DataStore(builder.Environment, builder.Configuration);
    Check(new WorkLocationService(restored).List(admin).Count == 1, "persistência após recarga");
    var now = DateTimeOffset.UtcNow;
    position.CapturedAt = now;
    var punchInput = new PunchInput { EmployeeId = staff.Id, Location = position };
    var captured = PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now);
    Check(captured.Status == "DentroDaArea" && captured.ReviewStatus == "NaoNecessaria", "captura validada na marcação");
    position.Longitude = 1;
    Reject(() => PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now), 400, "fora da área exige justificativa");
    punchInput.LocationJustification = "Atendimento externo autorizado";
    captured = PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now);
    Check(captured.Status == "ForaDaArea" && captured.ReviewStatus == "Pendente", "justificativa preserva pendência");
    position.CapturedAt = now.AddMinutes(-6);
    Check(PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now).Status == "CapturaDesatualizada", "captura antiga não valida presença");
    position.CapturedAt = now.AddMinutes(1);
    Check(PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now).Status == "CapturaDesatualizada", "captura futura não valida presença");
    position.CapturedAt = null;
    Reject(() => PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now), 400, "captura requer horário");
    punchInput.Location = null;
    Check(PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now).Status == "SemLocalizacao", "ausência de localização sinalizada");

    // Exercita o fluxo real do controller, incluindo escrita atômica e autorização.
    var controller = new WorkspaceController(store);
    void As(Guid actor) => controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
    {
        HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, actor.ToString()) }, "Test"))
        }
    };
    As(user);
    punchInput.LocationJustification = null;
    var countBefore = store.Read(db => db.Punches.Count);
    Reject(() => controller.RegisterPunch(punchInput), 400, "batida sem justificativa recusada atomicamente");
    Check(store.Read(db => db.Punches.Count) == countBefore, "falha não persiste marcação parcial");
    punchInput.LocationJustification = "Dispositivo sem localização disponível";
    var beforePunch = DateTimeOffset.UtcNow;
    var registered = (Punch)((Microsoft.AspNetCore.Mvc.OkObjectResult)controller.RegisterPunch(punchInput)).Value!;
    Check(registered.At >= beforePunch && registered.At <= DateTimeOffset.UtcNow && registered.Geolocation!.ReviewStatus == "Pendente", "batida com horário do servidor e análise pendente");
    var review = new ReviewPunchLocationRequest { Decision = "Recusada", Reason = "Local não autorizado para a atividade" };
    Reject(() => controller.ReviewPunchLocation(registered.Id, review), 403, "usuário não analisa justificativas");
    As(admin);
    controller.ReviewPunchLocation(registered.Id, review);
    var reviewed = store.Read(db => db.Punches.Single(x => x.Id == registered.Id));
    Check(reviewed.At == registered.At && reviewed.Geolocation!.Status == "SemLocalizacao" && reviewed.Geolocation.ReviewStatus == "Recusada", "recusa preserva marcação e resultado original");
    Reject(() => controller.ReviewPunchLocation(registered.Id, review), 400, "análise concluída não é sobrescrita");
    Check(new DataStore(builder.Environment, builder.Configuration).Read(db => db.Punches.Single(x => x.Id == registered.Id).Geolocation!.ReviewedBy) == admin, "análise persiste após recarga");
    service.Deactivate(admin, location.Id);
    Check(captured.WorkLocationName == "Escritório" && captured.RadiusMeters == 100, "snapshot independente do cadastro");
    Check(PunchGeolocationService.Capture(store.Read(db => db), staff, new PunchInput { EmployeeId = staff.Id }, now).Status == "SemRegra", "compatibilidade sem locais configurados");
    Check(validation.Validate(user, staff.Id, position).Status == "SemRegra", "local inativo não autoriza");
    position.AccuracyMeters = -1;
    Reject(() => validation.Validate(user, staff.Id, position), 400, "precisão negativa");

    var manager = new Account { Role = "Gestor", EmployeeId = staff.Id };
    var foreignTeam = new Team { Name = "Outro departamento" };
    var foreignStaff = new Staff { DepartmentId = foreignTeam.Id, Active = true };
    store.Write(db => { db.Accounts.Add(manager); db.Departments.Add(foreignTeam); db.Employees.Add(foreignStaff); db.Departments.Single(x => x.Id == team.Id).LeaderId = staff.Id; return true; });
    Reject(() => service.List(manager.Id), 403, "gestão desabilitada por padrão");
    store.Write(db => { db.GeolocationSettings.ManagersCanManageLocations = true; db.GeolocationSettings.ManagersCanReview = true; return true; });
    input.CompanyWide = false; input.DepartmentIds = new() { team.Id };
    var managed = service.Save(manager.Id, null, input);
    Check(service.List(manager.Id).Count == 1, "gestor lista apenas locais sob sua gestão");
    input.DepartmentIds = new() { foreignTeam.Id };
    Reject(() => service.Save(manager.Id, managed.Id, input), 403, "gestor não transfere local a outro departamento");
    input.CompanyWide = true;
    Reject(() => service.Save(manager.Id, null, input), 403, "gestor não libera local para toda empresa");
    Check(store.Read(db => GeolocationAccess.CanReview(db, manager, staff.Id) && !GeolocationAccess.CanReview(db, manager, foreignStaff.Id)), "análise do gestor limitada à liderança");
    position.Latitude = 0; position.Longitude = 1; position.AccuracyMeters = 5; position.CapturedAt = now.AddSeconds(-60);
    punchInput.Location = position; punchInput.LocationJustification = null;
    store.Write(db => { db.GeolocationSettings.RequireJustification = false; db.GeolocationSettings.MaxCaptureAgeSeconds = 10; return true; });
    var configurable = PunchGeolocationService.Capture(store.Read(db => db), staff, punchInput, now);
    Check(configurable.Status == "CapturaDesatualizada" && configurable.ReviewStatus == "Pendente", "limite e justificativa configuráveis");

    builder.Configuration["Geocoding:ReverseEndpoint"] = "https://geo.example.test/reverse";
    var handler = new FakeGeocodingHandler();
    using var client = new HttpClient(handler);
    var geocoder = new ReverseGeocodingService(client, builder.Configuration);
    var maintenance = new GeolocationMaintenanceService(store, geocoder);
    var toEnrich = new Punch { EmployeeId = staff.Id, At = now, Geolocation = captured };
    captured.ReceivedAt = now;
    store.Write(db => { db.GeolocationSettings.ReverseGeocodingEnabled = true; db.Punches.Add(toEnrich); return true; });
    Check(await maintenance.EnrichNextAsync(now, CancellationToken.None), "processamento assíncrono da captura");
    Check(store.Read(db => db.Punches.Single(p => p.Id == toEnrich.Id).Geolocation!.ApproximateLocation) == "Centro, João Pessoa, Paraíba, Brasil", "bairro e cidade persistidos");
    var cachedPunch = new Punch { At = now, Geolocation = new PunchGeolocation { ReceivedAt = now, Latitude = captured.Latitude, Longitude = captured.Longitude } };
    store.Write(db => { db.Punches.Add(cachedPunch); return true; });
    await maintenance.EnrichNextAsync(now, CancellationToken.None);
    Check(handler.Calls == 1, "resolução repetida usa cache persistido");
    handler.Status = System.Net.HttpStatusCode.ServiceUnavailable;
    var failedPunch = new Punch { At = now, Geolocation = new PunchGeolocation { ReceivedAt = now, Latitude = 10, Longitude = 10 } };
    store.Write(db => { db.Punches.Add(failedPunch); return true; });
    await maintenance.EnrichNextAsync(now, CancellationToken.None);
    Check(store.Read(db => db.Punches.Single(p => p.Id == failedPunch.Id).Geolocation!.GeocodingStatus) == "AguardandoNovaTentativa", "falha do provedor não remove batida");
    Check(!await maintenance.EnrichNextAsync(now, CancellationToken.None), "intervalo de tentativa respeitado");
    await maintenance.EnrichNextAsync(now.AddMinutes(6), CancellationToken.None);
    await maintenance.EnrichNextAsync(now.AddMinutes(17), CancellationToken.None);
    Check(store.Read(db => db.Punches.Single(p => p.Id == failedPunch.Id).Geolocation!.GeocodingStatus) == "Indisponivel", "tentativas limitadas após falha");
    handler.Status = System.Net.HttpStatusCode.OK; handler.Payload = "[]";
    Check(await geocoder.ResolveAsync(0, 0, CancellationToken.None) == null, "resposta fora do contrato tratada como indisponível");
    handler.Payload = "{\"address\":{}}";
    Check(await geocoder.ResolveAsync(0, 0, CancellationToken.None) == null, "resposta sem endereço não cria local fictício");
    Check(maintenance.PurgeExpired(now.AddDays(2)) == 0, "retenção desativada não remove dados");
    store.Write(db => { db.GeolocationSettings.RetentionDays = 1; return true; });
    Check(maintenance.PurgeExpired(now.AddDays(2)) > 0, "retenção remove localização vencida");
    var purged = store.Read(db => db.Punches.Single(p => p.Id == toEnrich.Id));
    Check(purged.At == now && purged.Geolocation!.PurgedAt != null && purged.Geolocation.Latitude == null && purged.Geolocation.ApproximateLocation == null && purged.Geolocation.Justification == null, "retenção preserva ponto e remove localização e texto");
    Check(!await maintenance.EnrichNextAsync(now.AddDays(2), CancellationToken.None), "captura removida não é geocodificada novamente");
    builder.Configuration["Geocoding:ReverseEndpoint"] = "https://nominatim.openstreetmap.org/reverse";
    Check(!geocoder.IsConfigured, "provedor público não recebe localizações de colaboradores");
}
finally { if (File.Exists(path)) File.Delete(path); }
