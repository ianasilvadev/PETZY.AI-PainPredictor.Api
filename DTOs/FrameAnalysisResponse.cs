using System;
using System.Collections.Generic;

namespace Petzy.FrameReceiver.DTOs.Analysis
{
    public class FrameAnalysisResponse
    {
        public List<AnimalDetectionDto> Animals { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }
}