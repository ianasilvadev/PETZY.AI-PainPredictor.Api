using Microsoft.AspNetCore.Mvc;
using Petzy.FrameReceiver.DTOs;
using Petzy.FrameReceiver.Models;
using Petzy.FrameReceiver.Services;
using Petzy.FrameReceiver.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Text;
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

        // ─────────────────────────────────────────────────────────────────
        // Recebe um frame, redimensiona para streaming, analisa e armazena
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ReceiveFrame([FromForm] FrameUploadRequest request)
        {
            if (request.Frame == null || request.Frame.Length == 0)
                return BadRequest("Frame inválido");

            if (request.Frame.Length > 5_000_000) // 5 MB
                return BadRequest("Frame muito grande");

            var frameId = Guid.NewGuid().ToString();

            using var memoryStream = new MemoryStream();
            await request.Frame.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            // --- Cria versão reduzida para streaming (480px, qualidade 50%) ---
            byte[] streamBytes;
            using (var img = Image.Load(bytes))
            {
                int maxWidth = 480; // reduzido para melhor performance
                if (img.Width > maxWidth)
                    img.Mutate(x => x.Resize(maxWidth, 0));
                using var outStream = new MemoryStream();
                img.Save(outStream, new JpegEncoder { Quality = 50 });
                streamBytes = outStream.ToArray();
            }

            // Análise de IA (pode demorar)
            var analysis = await _analysisService.AnalyzeFrameAsync(bytes);

            var frameData = new FrameData
            {
                FrameId = frameId,
                Image = bytes,
                StreamImage = streamBytes,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Analysis = analysis
            };

            FrameStore.AddFrame(frameId, frameData);

            Console.WriteLine($"Frame recebido: {frameId} | Original: {bytes.Length} bytes | Stream: {streamBytes.Length} bytes");

            return Ok(new
            {
                frameId,
                timestamp = frameData.Timestamp,
                analysis
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // Endpoints para frames individuais (consulta)
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public IActionResult GetFrame(string id)
        {
            if (!FrameStore.Frames.TryGetValue(id, out var frame))
                return NotFound("Frame não encontrado");

            return Ok(new
            {
                id,
                frame.Timestamp,
                frame.Analysis,
                ImageBase64 = Convert.ToBase64String(frame.Image)
            });
        }

        [HttpGet("{id}/image")]
        public IActionResult GetFrameImage(string id)
        {
            if (!FrameStore.Frames.TryGetValue(id, out var frame))
                return NotFound("Frame não encontrado");

            return File(frame.Image, "image/jpeg");
        }

        // ─────────────────────────────────────────────────────────────────
        // Endpoints para streaming em tempo real
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("stream/latest-frame")]
        public IActionResult GetLatestProcessedFrame()
        {
            if (FrameStore.LatestFrame == null)
                return NotFound("Nenhum frame disponível");

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            return File(FrameStore.LatestFrame.StreamImage, "image/jpeg");
        }

        [HttpGet("stream/latest-analysis")]
        public IActionResult GetLatestAnalysis()
        {
            if (FrameStore.LatestFrame == null)
                return NotFound("Nenhuma análise disponível");

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            return Ok(FrameStore.LatestFrame.Analysis);
        }

        [HttpGet("stream/mjpeg")]
        public async Task GetMJPEGStream()
        {
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            Response.Headers["Cache-Control"] = "no-cache";

            string lastFrameId = null;
            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                var latest = FrameStore.LatestFrame;
                if (latest != null && latest.FrameId != lastFrameId)
                {
                    lastFrameId = latest.FrameId;
                    byte[] imageData = latest.StreamImage ?? latest.Image;
                    await WriteFrameToResponse(imageData);
                }
                await Task.Delay(50); // ~20 FPS, ajustável
            }

            async Task WriteFrameToResponse(byte[] frameBytes)
            {
                var header = $"--frame\r\nContent-Type: image/jpeg\r\nContent-Length: {frameBytes.Length}\r\n\r\n";
                await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(header));
                await Response.Body.WriteAsync(frameBytes);
                await Response.Body.WriteAsync(Encoding.ASCII.GetBytes("\r\n"));
                await Response.Body.FlushAsync();
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Endpoint auxiliar: listar frames por data
        // ─────────────────────────────────────────────────────────────────
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
    }
}