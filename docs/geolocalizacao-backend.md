# Geolocalização no backend

## Estado atual

Cadastro de locais, permissões e validação integrados ao registro de ponto pelo app. Frontend integrado com área administrativa de geolocalização e captura na tela de ponto. O armazenamento continua em JSON, com uma empresa por instalação/arquivo. Não há suporte multiempresa no mesmo arquivo.

## Locais permitidos

Cada local contém nome, coordenadas, raio, situação ativa e permissões. As permissões para empresa inteira, departamento e colaborador são somadas. O Administrador gerencia todos os locais. A delegação ao Gestor é opcional e limitada aos departamentos em que ele é líder, conforme as regras abaixo.

- GET /api/work-locations: listar.
- POST /api/work-locations: criar (201).
- PUT /api/work-locations/{id}: substituir configuração.
- POST /api/work-locations/{id}/deactivate: desativar.
- POST /api/work-locations/validate/{employeeId}: simulação sem gravar ponto; Administrador ou próprio colaborador. Não avalia a idade da captura.

Todos os endpoints exigem autenticação. Vínculos devem existir. Nome entre 2 e 120 caracteres; raio maior que zero e até 100.000 metros; coordenadas finitas e válidas. Precisão finita e não negativa.

## Registro da batida

POST /api/workspace/punches continua recebendo employeeId, com location e locationJustification opcionais no contrato. Se location for informado, todos os dados da captura abaixo são exigidos.

```json
{
  "employeeId": "GUID-DO-COLABORADOR",
  "location": {
    "latitude": -7.12,
    "longitude": -34.86,
    "accuracyMeters": 20,
    "capturedAt": "2026-11-04T12:00:00Z"
  },
  "locationJustification": "Atendimento externo em cliente"
}
```

O horário oficial é definido pelo servidor. A posição e seu horário são dados declarados pelo dispositivo, sem garantia de autenticidade.

| Situação | Resultado |
| --- | --- |
| Distância mais precisão dentro de algum raio permitido | DentroDaArea; análise NaoNecessaria |
| Margem de precisão alcança uma área, sem inclusão completa | Inconclusivo; justificativa e análise |
| Fora de todas as áreas aplicáveis | ForaDaArea; justificativa e análise |
| Localização ausente com regras aplicáveis | SemLocalizacao; justificativa e análise |
| Captura anterior a cinco minutos ou mais de 30 segundos no futuro, com regras aplicáveis | CapturaDesatualizada; justificativa e análise |
| Nenhum local ativo aplicável | SemRegra; não considera a localização validada e não exige justificativa |

Por padrão, as situações que exigem análise precisam de justificativa com 5 a 500 caracteres após remoção de espaços externos. O administrador pode tornar a justificativa opcional; a pendência de análise permanece. Sem justificativa, a API retorna 400 e não grava batida parcial. O cliente deve reenviar a solicitação completa com justificativa; o horário oficial será o do registro aceito, sem retroatividade. Coordenadas inválidas ou captura sem horário retornam 400.

Os limites temporais são configuráveis. O tamanho máximo da justificativa continua em 500 caracteres. A precisão é considerada geometricamente; não há limite absoluto de precisão nesta etapa.

Os dados ficam em punches[].geolocation, inclusive nas consultas existentes do workspace. São salvos posição, precisão, captura, recebimento, situação, justificativa, distância e cópia do nome, coordenadas e raio do local usado. Alterar/desativar o local não recalcula o histórico. Marcações antigas e faciais permanecem com geolocation nulo. O terminal facial não participa desse fluxo.

## Análise administrativa

POST /api/workspace/punches/{id}/location-review, para Administrador ou Gestor com delegação e liderança no departamento do colaborador:

```json
{ "decision": "Aceita", "reason": "Atendimento externo confirmado" }
```

Decisões permitidas: Aceita e Recusada. Motivo entre 5 e 500 caracteres. Apenas pendências podem ser analisadas. São salvos autor, horário e motivo, com auditoria. Uma análise concluída não pode ser sobrescrita por esse endpoint. Recusar a justificativa preserva a marcação, o horário, a captura e a classificação geográfica original; não altera cálculo de horas nem apaga o ponto.

## Organização

- Controllers/WorkLocationsController.cs: endpoints de locais e simulação.
- Controllers/WorkspaceController.cs: integração da captura e análise à batida.
- DTOs/Geolocation: contratos de locais, captura e análise.
- Operations/WorkLocation.cs: locais e permissões.
- Operations/PunchGeolocation.cs: snapshot da captura e análise.
- Operations/Models.cs: coleção de locais e extensão dos contratos de ponto.
- Services/WorkLocationService.cs: cadastro, validações de vínculos e auditoria.
- Services/GeolocationValidationService.cs: cálculo de distância e regras geográficas.
- Services/PunchGeolocationService.cs: captura, justificativa e snapshot na mesma transação do ponto.
- Operations/DataStore.cs: persistência atômica existente.

## Verificação

```powershell
dotnet run --no-restore --project tests/Geolocation/Geolocation.Tests.csproj
```

Testes com dados temporários isolados cobrem permissões, distância, precisão, captura antiga/futura, justificativa, integração com registro, análise, ausência de gravação parcial e persistência após recarga.

### Testes HTTP

```powershell
./scripts/Test-Geolocation.ps1
# Porta alternativa, caso 5103 esteja ocupada:
./scripts/Test-Geolocation.ps1 -Port 5104
```

O script compila o backend e inicia uma API real com armazenamento, chaves de autenticação e logs em uma pasta exclusiva de artifacts/geolocation-http-{guid}. Não utiliza os dados da execução normal. Encerra o processo iniciado ao finalizar, inclusive em caso de falha, e preserva os arquivos de diagnóstico em artifacts, ignorado pelo Git.

São 106 verificações de respostas HTTP e conteúdo: autenticação por cookie, restrições administrativas, isolamento dos registros por colaborador, contratos JSON inválidos, permissões por empresa/departamento/colaborador, posição dentro/fora/inconclusiva, justificativa, horário do servidor, análise administrativa, auditoria e persistência após reinício. O teste também confirma que requisições rejeitadas não deixam marcações parciais e que mudanças no local não alteram o snapshot. A rejeição de justificativa é coberta adicionalmente pelos testes de serviços/controller; o fluxo HTTP cobre o aceite.


## Configuração por instalação da empresa

GET e PUT /api/geolocation/settings, exclusivos do Administrador. PUT substitui a política inteira: campos omitidos voltam aos padrões. Os valores são persistidos no JSON atual.

```json
{
  "maxCaptureAgeSeconds": 300,
  "futureToleranceSeconds": 30,
  "requireJustification": true,
  "managersCanManageLocations": false,
  "managersCanReview": false,
  "retentionDays": null,
  "reverseGeocodingEnabled": false
}
```

Idade máxima entre 1 e 3600 segundos, tolerância futura entre 0 e 300 segundos. Retenção nula significa limpeza desativada; um prazo configurado deve ter entre 1 e 36500 dias. A empresa deve escolher seu prazo, sem um valor de retenção imposto pelo sistema.

## Delegação ao Gestor

A liderança é identificada por Departments.LeaderId ligado ao EmployeeId da conta Gestor ativa. O administrador mantém acesso integral.

- ManagersCanManageLocations: o gestor pode criar, listar, editar e desativar apenas locais restritos integralmente à sua abrangência. Não pode gerenciar locais liberados para a empresa inteira ou que também incluam departamentos/colaboradores fora de sua liderança. A edição verifica tanto o local existente quanto a nova abrangência.
- ManagersCanReview: o gestor pode consultar e analisar pendências de colaboradores pertencentes aos departamentos liderados.
- GET /api/geolocation/reviews?page=1: fila paginada com total, page, pageSize=50 e items. Administradores veem todas as pendências; gestores veem somente sua abrangência. Usuários comuns não acessam a fila.
- A delegação não concede acesso às configurações, ao provedor ou a outras áreas administrativas do sistema.

O perfil Gestor não recebe automaticamente permissões de líder: é necessário configurar o vínculo de liderança e habilitar a delegação.

## Local aproximado e integração externa

ReverseGeocodingService implementa o contrato de geocodificação reversa Nominatim jsonv2 para um servidor próprio ou fornecedor contratado compatível. Não há provedor externo configurado ou contratado pelo repositório. Nenhuma coordenada é enviada enquanto a opção estiver desativada ou não houver um endpoint válido.

Na implantação, configure um endpoint HTTPS próprio/contratado. Exemplo de variável de ambiente, com endereço fictício a substituir:

```powershell
$env:Geocoding__ReverseEndpoint = 'https://geo.sua-empresa.example/reverse'
```

Depois, habilite reverseGeocodingEnabled na política pelo PUT /api/geolocation/settings. O endpoint é configuração de implantação, não uma URL recebida do colaborador. Redirecionamentos não são seguidos. O servidor público nominatim.openstreetmap.org é recusado: o serviço público orienta não enviar dados pessoais/confidenciais. Use servidor próprio ou fornecedor cujo uso esteja autorizado pela empresa.

- GET /api/geolocation/provider-status: informa configured e enabled, sem expor URL ou segredos; exclusivo do administrador.
- GeolocationWorker processa uma captura por ciclo de 15 segundos, sem bloquear a batida. A consulta tem timeout de 5 segundos e resposta limitada a 64 KB.
- São extraídos bairro, cidade, estado e país; número residencial e rua não são armazenados pelo resolvedor.
- Campos: approximateLocation, attribution, geocodingStatus, geocodingAttempts e nextGeocodingAttemptAt.
- Situações: Pendente, Resolvida, AguardandoNovaTentativa, Indisponivel e Removida.
- Até três tentativas automáticas, com espera de 5 e 10 minutos entre falhas. A fila é derivada das marcações persistidas e sobrevive ao reinício.
- Consultas das mesmas coordenadas reaproveitam resultados persistidos das últimas 24 horas, respeitando a retenção. Não há aproximação por arredondamento de coordenadas.
- POST /api/geolocation/punches/{id}/retry-geocoding: administrador pode solicitar novas tentativas de uma captura sem resultado; exige provedor configurado e habilitado. Responde 409 se a integração estiver desativada ou não configurada.
- Falha do provedor não desfaz ponto, não altera a validação geográfica e não impede análise. Logs do cliente HTTP de geocodificação estão desativados para não registrar coordenadas na URL.
- A atribuição do provedor é persistida e deve ser apresentada pelo consumidor junto do endereço aproximado. O nome retornado não comprova o endereço exato da captura.

Contrato do provedor: https://nominatim.org/release-docs/latest/api/Reverse/
Política do serviço público: https://operations.osmfoundation.org/policies/nominatim/

## Retenção

Quando retentionDays estiver configurado, o worker limpa localizações vencidas no próximo ciclo, usando ReceivedAt, sem apagar a marcação ou alterar seu horário.

São removidos coordenadas, precisão, horário declarado da captura, referência/nome/coordenadas/raio do local, distância, endereço aproximado, atribuição, justificativa e motivo textual da análise. São mantidos classificação, recebimento, decisão, autor e horário da análise. PurgedAt registra a limpeza. Pendências vencidas recebem ReviewStatus=Expirada, deixam a fila e não podem ser analisadas pelo fluxo normal. A limpeza também remove motivos antigos de análise que tenham sido copiados para a auditoria.

A limpeza não remove locais permitidos cadastrados. Não alcança backups exportados, logs externos ou cópias anteriores; a política de armazenamento desses arquivos continua a cargo da implantação. Desativar a retenção depois não recupera dados já removidos. Capturas limpas não voltam a ser geocodificadas.

## Testes adicionais

O projeto tests/Geolocation cobre também delegação e escopo de liderança, configurações, resolução com provedor simulado, cache, resposta sem endereço, falhas, tentativas limitadas e limpeza por retenção. Nenhuma coordenada real é enviada a serviço externo nos testes. Os testes HTTP cobrem também configuração, delegação, fila administrativa e rejeição de reprocessamento sem provedor.

## Ativação e limites

O código das pendências do backend está implementado. Para ativar em cada instalação, a empresa precisa configurar seu provedor, habilitar a consulta externa, escolher o prazo de retenção e decidir a delegação. Sem essas configurações, os padrões preservam o fluxo anterior. O provedor real deverá ser verificado no ambiente de implantação; os testes locais usam respostas simuladas.

A arquitetura continua com uma empresa por instalação. Compartilhar o mesmo armazenamento entre várias empresas exige um projeto separado de isolamento de dados. O frontend envia captura/justificativa na tela de ponto e oferece uma área administrativa em /geolocalizacao. O roteiro de teste está em docs/geolocalizacao-teste-manual.md.
