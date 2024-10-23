using System.IO.Pipes;
using App.Util;

namespace App.Communication;

public sealed class ReversedPipeClient(uint pid) : IAsyncDisposable
{
    private const string PipeName = "dst_console_pipe_";
    private readonly NamedPipeClientStream _pipeClient = new(PipeName + pid);
    private CancellationTokenSource? _cts;

    public delegate void MessageReceivedHandler(PipeMessage message);

    public event MessageReceivedHandler? MessageReceived;

    public void StartListening()
    {
        _cts = new CancellationTokenSource();
        Task.Factory.StartNew(async () =>
        {
            await _pipeClient.ConnectAsync(_cts.Token);
            var buffer = new byte[6144];
            while (!_cts.Token.IsCancellationRequested)
            {
                if (!_pipeClient.IsConnected)
                {
                    Console.WriteLine("Pipe disconnected!");
                    ManagedConsole.MaskConsoleInvalid(pid);
                    break;
                }

                _ = await _pipeClient.ReadAsync(buffer, _cts.Token);
                var message = new PipeMessage(buffer);

                MessageReceived?.Invoke(message);

                Console.WriteLine($"Received message: {message}");
            }
        }, _cts.Token);
    }

    public void StopListening()
    {
        _cts?.Cancel();
        _pipeClient.Close();
    }

    public async ValueTask DisposeAsync()
    {
        await _cts?.CancelAsync()!;
        await _pipeClient.DisposeAsync();
    }
}