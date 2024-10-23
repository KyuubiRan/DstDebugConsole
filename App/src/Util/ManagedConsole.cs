using App.Communication;
using App.Ui;

namespace App.Util;

public static class ManagedConsole
{
    private static Dictionary<uint, ConsoleWindow> _windows = new();

    public static void MaskConsoleInvalid(uint pid)
    {
        if (_windows.TryGetValue(pid, out var window))
        {
            window.IsAlive = false;
            _windows.Remove(pid);
            PipeManager.StopListeningForProcess(pid);
        }
    }

    public static void DeleteWindow(uint pid)
    {
        if (_windows.TryGetValue(pid, out var window))
        {
            window.IsShow = false;
            _windows.Remove(pid);
            PipeManager.StopListeningForProcess(pid);
        }
    }
    
    public static void CreateWindow(ProcessWatcher.ProcessEventArgs eventArgs)
    {
        var ret = DllInjector.Inject(eventArgs.ProcessId, "LibConsole.dll");
        if (ret != DllInjector.InjectResult.Success)
        {
            Console.WriteLine("Inject failed: " + ret);
            return;
        }
        
        var window = new ConsoleWindow(eventArgs);
        _windows.Add(eventArgs.ProcessId, window);
        PipeManager.ListenForProcess(eventArgs.ProcessId, message => window.PushMessage(message));
        Console.WriteLine($"Created console window for PID={eventArgs.ProcessId}");
    }
}