using System.Net.Http;
using System.Threading.Tasks;

public class PainModelService
{
    private readonly HttpClient _httpClient;

    public PainModelService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PainPredictionResponse?> PredictPainAsync(IFormFile imageFile)
    {
        using var content = new MultipartFormDataContent();
        using var fileStream = imageFile.OpenReadStream();

        var imageContent = new StreamContent(fileStream);
        imageContent.Headers.ContentType = 
            new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);

        content.Add(imageContent, "file", imageFile.FileName);

        var response = await _httpClient.PostAsync("/predict", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<PainPredictionResponse>(json);
    }
}

public class PainPredictionResponse
{
    public float saida_bruta { get; set; }
    public bool dor { get; set; }
}
