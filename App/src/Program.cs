// See https://aka.ms/new-console-template for more information

#pragma warning disable CA1416

using System.Diagnostics;
using App.Ui;
using App.Util;

HashSet<string> watchedProcesses = ["dontstarve_steam_x64.exe", "dontstarve_dedicated_server_nullrenderer_x64.exe"];

{
    var procs = Process.GetProcesses();
    var procNames = watchedProcesses.Select(x => x[..^4]).ToHashSet();
    foreach (var proc in procs)
    {
        // Console.WriteLine($"Found process: {proc.ProcessName}(PID={proc.Id})");
        if (procNames.Contains(proc.ProcessName))
        {
            ManagedConsole.CreateWindow(new ProcessWatcher.ProcessEventArgs(proc.ProcessName, (uint)proc.Id, true));
            Console.WriteLine($"Found dst process: {proc.ProcessName}(PID={proc.Id})");
        }
    }
}

ProcessWatcher.ProcessEvent += (sender, eventArgs) =>
{
    if (eventArgs.IsStart == false || !watchedProcesses.Contains(eventArgs.ProcessName))
        return;

    Console.WriteLine($"Found dst process: {eventArgs.ProcessName}(PID={eventArgs.ProcessId})");
    ManagedConsole.CreateWindow(eventArgs);
};
ProcessWatcher.StartWatching();
_ = new MainWindow();
await MainOverlay.Instance.Run();