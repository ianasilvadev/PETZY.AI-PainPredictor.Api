namespace Petzy.FrameReceiver.DTOs;

public class FrameUploadRequest
{
    public DateTime? Timestamp { get; set; }
    public IFormFile Frame { get; set; } = null!;
}