using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CsClientGenerator;
class OwnersClient
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private static readonly string Host = "http://localhost:8080";
    public static async Task<List<Owner>?> allOwners()
    {
        var url = $"/getAll";
        var response = await HttpClient.GetAsync(Host + url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Owner>>(content);
    }

    public static async Task createOwner(Owner owner)
    {
        var url = $"/create";
        var json = JsonConvert.SerializeObject(owner);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await HttpClient.PostAsync(Host + url, content);
    }

    public static async Task<Owner?> getOwner(int id)
    {
        var url = $"/read/{id}";
        var response = await HttpClient.GetAsync(Host + url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Owner>(content);
    }

    public static async Task updateOwner(int id, Owner owner)
    {
        var url = $"/update/{id}";
        var json = JsonConvert.SerializeObject(owner);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await HttpClient.PatchAsync(Host + url, content);
    }

    public static async Task deleteOwner(int id)
    {
        var url = $"/delete/{id}";
        var content = new StringContent("", Encoding.UTF8);
        await HttpClient.DeleteAsync(Host + url);
    }

    public static async Task addKitten(int id, int kittenId)
    {
        var url = $"/addKitten";
        var content = new StringContent($"?id={id}&kittenId={kittenId}", Encoding.UTF8);
        await HttpClient.PatchAsync(Host + url, content);
    }
}