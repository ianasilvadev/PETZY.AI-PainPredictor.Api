using System.Collections.Generic;

namespace Petzy.FrameReceiver.Storage
{
    public static class FrameStore
    {
        public static Dictionary<string, FrameData> Frames { get; } = new();
        public static FrameData? LatestFrame { get; private set; }

        public static void AddFrame(string id, FrameData frame)
        {
            lock (Frames) // thread-safe para concorrência
            {
                Frames[id] = frame;
                LatestFrame = frame;
            }
        }
    }
}