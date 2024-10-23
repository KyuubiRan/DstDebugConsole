using System.Management;
using System.Runtime.Versioning;

namespace App.Util;

public static class ProcessWatcher
{
    public class ProcessEventArgs(string processName, uint processId, bool isStart) : EventArgs
    {
        public bool IsStart { get; } = isStart;
        public string ProcessName { get; } = processName;
        public uint ProcessId { get; } = processId;
    }
    
    public delegate void ProcessEventHandler(object sender, ProcessEventArgs e);

    public static ProcessEventHandler? ProcessEvent;
    
    static ProcessWatcher()
    {
#if DEBUG
        // ProcessEvent += delegate(object sender, ProcessEventArgs e)
        // {
        //     Console.WriteLine(
        //         $"ProcessEvent: {e.ProcessName}(PID={e.ProcessId}) {(e.IsStart ? "started" : "exited")}");
        // };
#endif
    }
    
    [SupportedOSPlatform("windows")]
    public static void StartWatching()
    {
        {
            // Start event
            var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            var watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += HandleStartEvent;
            watcher.Start();
        }
        {
            // Exit event
            var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
            var watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += HandleExitEvent;
            watcher.Start();
        }
    }
    
    [SupportedOSPlatform("windows")]
    private static void HandleStartEvent(object sender, EventArrivedEventArgs e)
    {
        var procname = (string)e.NewEvent.Properties["ProcessName"].Value;
        var procid = (uint)e.NewEvent.Properties["ProcessID"].Value;

        ProcessEvent?.Invoke(sender, new ProcessEventArgs(procname, procid, true));
    }

    [SupportedOSPlatform("windows")]
    private static void HandleExitEvent(object sender, EventArrivedEventArgs e)
    {
        var procname = (string)e.NewEvent.Properties["ProcessName"].Value;
        var procid = (uint)e.NewEvent.Properties["ProcessID"].Value;

        ProcessEvent?.Invoke(sender, new ProcessEventArgs(procname, procid, false));
    }
}
