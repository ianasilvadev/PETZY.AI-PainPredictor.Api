using System.Collections.Generic;

namespace Petzy.FrameReceiver.DTOs.Analysis
{
    public class AnimalDetectionDto
    {
        public int TrackId { get; set; }
        public string Species { get; set; } = string.Empty;
        public List<int> Bbox { get; set; } = new(); // [x, y, width, height]
        public double FaceScore { get; set; }
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }
}