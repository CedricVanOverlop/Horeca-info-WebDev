namespace Infrastructure.Utils;

static public class Base64Utils
{
    static public string? GetBase64Image(string? imageUrl)
    {
        using var httpClient = new HttpClient();

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        try
        {
            var response = httpClient.GetAsync(imageUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                return Convert.ToBase64String(imageBytes);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
