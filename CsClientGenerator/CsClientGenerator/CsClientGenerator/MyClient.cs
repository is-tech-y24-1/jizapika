using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CsClientGenerator;

public class MyClient
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private static readonly string Host = "http://localhost:8080";

    public static async Task create(MyKitten kitten)
    {
        var url = $"/clients";
        var json = JsonConvert.SerializeObject(kitten);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await HttpClient.PostAsync(Host + url, content);
    }
    public static async Task<List<MyKitten>?> read()
    {
        var url = $"/clients";
        var response = await HttpClient.GetAsync(Host + url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<MyKitten>>(content);
    }
    public static async Task<MyKitten?> read(int id)
    {
        var url = $"/clients/{id}";
        var response = await HttpClient.GetAsync(Host + url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MyKitten>(content);
    }
    public static async Task update(int id, MyKitten kitten)
    {
        var url = $"/clients/{id}";
        var json = JsonConvert.SerializeObject(kitten);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await HttpClient.PutAsync(Host + url, content);
    }
    public static async Task delete(int id)
    {
        var url = $"/clients/{id}";
        await HttpClient.DeleteAsync(Host + url);
    }
    
    public static async Task addFriend(int id, int kittenId)
    {
        var url = $"/clients";
        var content = new StringContent($"?id={id}&kittenId={kittenId}", Encoding.UTF8);
        await HttpClient.PatchAsync(Host + url, content);
    }
}