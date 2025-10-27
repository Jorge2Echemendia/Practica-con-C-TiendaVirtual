using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TiendaVirtual.Service;

public class ImagenService
{
    private readonly string _uploadUrl = "https://upload.imagekit.io/api/v1/files/upload";
    private readonly string _privateKey;
    private readonly string _urlEndpoint;

    public ImagenService(IConfiguration config)
    {
        _privateKey = config["ImageKit:PrivateKey"];
        _urlEndpoint = config["ImageKit:UrlEndpoint"];
    }

    public async Task<string> SubirImagenAsync(Stream archivoStream, string nombreArchivo)
    {
        using var client = new HttpClient();
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_privateKey}:");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(archivoStream), "file", nombreArchivo);
        content.Add(new StringContent(nombreArchivo), "fileName");

        var response = await client.PostAsync(_uploadUrl, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("url").GetString();
    }
}