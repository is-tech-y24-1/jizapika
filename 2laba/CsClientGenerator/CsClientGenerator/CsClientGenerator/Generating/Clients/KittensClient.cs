using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CsClientGenerator;
class KittensClient
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private static readonly string Host = "http://localhost:8080";
    public static async Task<List<Kitten>?> allKittens()
    {
        var url = $"/getAll";
        var response = await HttpClient.GetAsync(Host + url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Kitten>>(content);
    }

    public static async Task createKitten(Kitten kitten)
    {
        var url = $"/create";
        var json = JsonConvert.SerializeObject(kitten);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await HttpClient.PostAsync(Host + url, content);
    }

    public static async Task<Kitten?> getKitten(int id)
    {
        var url = $"/read/{id}";
        var response = await HttpClient.GetAsync(Host + url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Kitten>(content);
    }

    public static async Task updateKitten(int id, Kitten kitten)
    {
        var url = $"/update/{id}";
        var json = JsonConvert.SerializeObject(kitten);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await HttpClient.PatchAsync(Host + url, content);
    }

    public static async Task deleteKitten(int id)
    {
        var url = $"/delete/{id}";
        var content = new StringContent("", Encoding.UTF8);
        await HttpClient.DeleteAsync(Host + url);
    }

    public static async Task addFriend(int catId, int friendId)
    {
        var url = $"/addFriend";
        var content = new StringContent($"?catId={catId}&friendId={friendId}", Encoding.UTF8);
        await HttpClient.PatchAsync(Host + url, content);
    }
}