using Petzy.FrameReceiver.DTOs.Analysis;

public class FrameData
{
     public string? FrameId { get; set; }
    public byte[]? Image { get; set; }          // original (para consulta)
    public byte[]? StreamImage { get; set; }    // versão reduzida para streaming
    public DateTime Timestamp { get; set; }
    public FrameAnalysisResponse Analysis { get; set; }
}