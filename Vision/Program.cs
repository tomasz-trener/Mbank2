using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var endpoint = "";
        var apiKey = "";
        var imageUrl = "https://idodata.com/wp-content/uploads/2023/12/ComputerOperator-scaled.jpg";

        // ✅ Prawidłowy URL z parametrami w query string
        var apiUrl = $"{endpoint}/computervision/imageanalysis:analyze?api-version=2024-02-01" +
                     $"&features=caption,denseCaptions,tags,read,people,objects" +
                     $"&language=en" +
                     $"&genderNeutralCaption=false";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // JSON zawiera tylko obraz (źródło)
        var requestBody = new { url = imageUrl };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(apiUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Analysis failed.");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Response: {responseContent}");
            return;
        }

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;

        if (root.TryGetProperty("captionResult", out var captionResult))
        {
            var caption = captionResult.GetProperty("text").GetString();
            Console.WriteLine($"Caption: {caption}");
        }

        if (root.TryGetProperty("denseCaptionsResult", out var denseCaptions))
        {
            foreach (var caption in denseCaptions.GetProperty("values").EnumerateArray())
            {
                Console.WriteLine($"Dense Caption: {caption.GetProperty("text").GetString()}, confidence: {caption.GetProperty("confidence").GetDouble()}");
            }
        }

        if (root.TryGetProperty("tagsResult", out var tagsResult))
        {
            foreach (var tag in tagsResult.GetProperty("values").EnumerateArray())
            {
                Console.WriteLine($"Tag: {tag.GetProperty("name").GetString()}");
            }
        }

        if (root.TryGetProperty("peopleResult", out var peopleResult))
        {
            foreach (var person in peopleResult.GetProperty("values").EnumerateArray())
            {
                Console.WriteLine($"Person confidence: {person.GetProperty("confidence").GetDouble()}");
            }
        }



        if (root.TryGetProperty("readResult", out var readResult))
        {
            foreach (var block in readResult.GetProperty("blocks").EnumerateArray())
            {
                foreach (var line in block.GetProperty("lines").EnumerateArray())
                {
                    Console.WriteLine($"Line: {line.GetProperty("text").GetString()}");

                    foreach (var word in line.GetProperty("words").EnumerateArray())
                    {
                        Console.WriteLine($"  Word: {word.GetProperty("text").GetString()}");
                    }
                }
            }
        }
    }
}
