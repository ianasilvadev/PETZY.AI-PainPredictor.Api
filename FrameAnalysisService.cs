using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Petzy.FrameReceiver.DTOs.Analysis;

namespace Petzy.FrameReceiver.Services
{
    public class FrameAnalysisService : IFrameAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrameAnalysisService> _logger;
        private const string PythonApiUrl = "http://localhost:8000/analyze-frame";

        public FrameAnalysisService(HttpClient httpClient, ILogger<FrameAnalysisService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FrameAnalysisResponse?> AnalyzeFrameAsync(byte[] imageBytes)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(imageContent, "file", "frame.jpg");

                var response = await _httpClient.PostAsync(PythonApiUrl, content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var analysis = JsonSerializer.Deserialize<FrameAnalysisResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return analysis;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Falha ao chamar o serviço Python de análise. Análise será nula.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao analisar frame.");
                return null;
            }
        }
    }
}