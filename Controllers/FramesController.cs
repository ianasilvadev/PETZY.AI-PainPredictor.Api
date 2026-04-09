using Microsoft.AspNetCore.Mvc;
using Petzy.FrameReceiver.DTOs;
using Petzy.FrameReceiver.Models;
using Petzy.FrameReceiver.Services;
using Petzy.FrameReceiver.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Petzy.FrameReceiver.Controllers
{
    [ApiController]
    [Route("frames")]
    public class FramesController : ControllerBase
    {
        private readonly IFrameAnalysisService _analysisService;

        public FramesController(IFrameAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveFrame([FromForm] FrameUploadRequest request)
        {
            if (request.Frame == null || request.Frame.Length == 0)
                return BadRequest("Frame inválido");

            if (request.Frame.Length > 1_000_000)
                return BadRequest("Frame muito grande");

            var frameId = Guid.NewGuid().ToString();

            using var memoryStream = new MemoryStream();
            await request.Frame.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            // Chamar serviço de IA
            var analysis = await _analysisService.AnalyzeFrameAsync(bytes);

            // Salvar frame com análise (pode ser nula se o serviço estiver offline)
            FrameStore.Frames[frameId] = new FrameData
            {
                Image = bytes,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Analysis = analysis
            };

            Console.WriteLine($"Frame recebido: {frameId}");
            Console.WriteLine($"Tamanho: {bytes.Length} bytes");
            Console.WriteLine($"Análise: {(analysis == null ? "não obtida" : "OK")}");

            // Retornar o ID e a análise (se houver)
            return Ok(new
            {
                frameId,
                timestamp = FrameStore.Frames[frameId].Timestamp,
                analysis
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetFrame(string id)
        {
            if (!FrameStore.Frames.TryGetValue(id, out var frame))
                return NotFound("Frame não encontrado");

            // Retorna os metadados + imagem em base64
            return Ok(new
            {
                id,  // o parâmetro da rota é o ID
                frame.Timestamp,
                frame.Analysis,
                ImageBase64 = Convert.ToBase64String(frame.Image)
            });
        }

        [HttpGet("latest")]
        public IActionResult GetLatestFrame()
        {
            if (!FrameStore.Frames.Any())
                return NotFound("Nenhum frame disponível");

            var last = FrameStore.Frames.Last(); // KeyValuePair<string, FrameData>
            return Ok(new
            {
                id = last.Key,
                last.Value.Timestamp,
                last.Value.Analysis,
                ImageBase64 = Convert.ToBase64String(last.Value.Image)
            });
        }

        [HttpGet("by-date")]
        public IActionResult GetAllFramesByDate(DateTime date)
        {
            var framesDate = FrameStore.Frames
                .Where(f => f.Value.Timestamp.Date == date.Date)
                .Select(f => new
                {
                    id = f.Key,
                    timestamp = f.Value.Timestamp,
                    analysis = f.Value.Analysis,
                    url = $"{Request.Scheme}://{Request.Host}/frames/{f.Key}"
                })
                .ToList();

            return Ok(framesDate);
        }

        // Opcional: endpoint para obter apenas a imagem (sem JSON)
        [HttpGet("{id}/image")]
        public IActionResult GetFrameImage(string id)
        {
            if (!FrameStore.Frames.TryGetValue(id, out var frame))
                return NotFound("Frame não encontrado");

            return File(frame.Image, "image/jpeg");
        }
    }
}