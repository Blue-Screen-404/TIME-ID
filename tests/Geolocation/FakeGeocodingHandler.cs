using System.Net;
using TimeId.Services;

public sealed class FakeGeocodingHandler : HttpMessageHandler
{
    public int Calls { get; private set; }
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public string Payload { get; set; } = """{"address":{"suburb":"Centro","city":"João Pessoa","state":"Paraíba","country":"Brasil"},"licence":"OSM teste"}""";
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        if (request.RequestUri?.Query.Contains("lat=") != true) throw new Exception("Coordenadas ausentes da consulta.");
        return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(Payload) });
    }
}
