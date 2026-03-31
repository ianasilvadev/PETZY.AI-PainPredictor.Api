using Microsoft.AspNetCore.Mvc;
using Petzy.FrameReceiver.DTOs;
using Petzy.FrameReceiver.Storage;
using Petzy.FrameReceiver.Models;

namespace Petzy.FrameReceiver.Controllers;

[ApiController]
[Route("frames")]
public class FramesController : ControllerBase
{
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

        FrameStore.Frames[frameId] = new FrameData
        {
            Image = bytes,
            Timestamp = request.Timestamp ?? DateTime.UtcNow
        };

        Console.WriteLine($"Frame recebido");
        Console.WriteLine($"Tamanho: {bytes.Length} bytes");
        Console.WriteLine($"Timestamp salvo: {FrameStore.Frames[frameId].Timestamp}");

        return Ok(new
        {
            message = "Frame recebido com sucesso",
            frameId = frameId
        });
    }

    [HttpGet("{id}")]
    public IActionResult GetFrame(string id)
    {
        if (!FrameStore.Frames.TryGetValue(id, out var frame))
            return NotFound("Frame não encontrado");

        return File(frame.Image, "image/jpeg");
    }

    [HttpGet("latest")]
    public IActionResult GetLatestFrame()
    {
        if (!FrameStore.Frames.Any())
            return NotFound("Nenhum frame disponível");

        var last = FrameStore.Frames.Last();

        return File(last.Value.Image, "image/jpeg");
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
                url = $"{Request.Scheme}://{Request.Host}/frames/{f.Key}"
            })
            .ToList();

        return Ok(framesDate);
    }
}