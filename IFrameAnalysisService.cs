using System.Threading.Tasks;
using Petzy.FrameReceiver.DTOs.Analysis;

namespace Petzy.FrameReceiver.Services
{
    public interface IFrameAnalysisService
    {
        Task<FrameAnalysisResponse?> AnalyzeFrameAsync(byte[] imageBytes);
    }
}