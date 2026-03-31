using System.Collections.Concurrent;
using Petzy.FrameReceiver.Models;

namespace Petzy.FrameReceiver.Storage;

public static class FrameStore
{
    public static ConcurrentDictionary<string, FrameData> Frames = new();
}