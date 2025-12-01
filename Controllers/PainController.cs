using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using PainPredictor.Api.DTOs;

namespace PainPredictor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PainController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public PainController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    // ============================================
    //          POST /api/pain/predizer
    // ============================================
    [HttpPost("predizer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Predizer([FromForm] PainPredictRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Nenhuma imagem enviada." });

        try
        {
            using var content = new MultipartFormDataContent();

            var imgStream = new StreamContent(file.OpenReadStream());
            imgStream.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(imgStream, "file", file.FileName);

            // URL do modelo Python
            var pythonUrl = "http://localhost:8000/predict";

            var response = await _httpClient.PostAsync(pythonUrl, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Erro ao acessar o modelo de dor.");

            var json = await response.Content.ReadAsStringAsync();

            // JSON já está em formato correto
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
