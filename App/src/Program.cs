// See https://aka.ms/new-console-template for more information

#pragma warning disable CA1416

using App.Ui;
using App.Util;

var watchedProcesses = new List<string>
    { "dontstarve_steam_x64.exe", "dontstarve_dedicated_server_nullrenderer_x64.exe" };

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