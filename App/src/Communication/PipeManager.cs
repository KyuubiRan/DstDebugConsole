using System.Collections.Concurrent;

#pragma warning disable CA1416

namespace App.Communication;

public static class PipeManager
{
    private static ConcurrentDictionary<uint, ReversedPipeClient> _pipes = new();

    public static void ListenForProcess(uint pid, Action<PipeMessage> onMessage)
    {
        if (_pipes.ContainsKey(pid))
            return;

        var pipe = new ReversedPipeClient(pid);
        pipe.MessageReceived += message => onMessage(message);
        pipe.StartListening();

        _pipes.TryAdd(pid, pipe);
    }

    public static void StopListeningForProcess(uint pid)
    {
        if (_pipes.TryRemove(pid, out var pipe))
        {
            pipe.StopListening();
        }
    }
}