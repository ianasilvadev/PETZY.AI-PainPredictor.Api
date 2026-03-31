namespace Petzy.FrameReceiver.Models;

public class FrameData
{
    public byte[] Image { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}