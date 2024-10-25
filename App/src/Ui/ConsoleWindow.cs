using System.Collections.Concurrent;
using System.Numerics;
using App.Communication;
using App.Util;
using FoxTail.Extensions;
using ImGuiNET;

namespace App.Ui;

public class ConsoleWindow(ProcessWatcher.ProcessEventArgs eventArgs) : BaseWindow
{
#if DEBUG
    public bool IsDebugAlloc = false;
#endif
    public bool IsAlive { get; set; } = true;
    public bool IsShow = true;
    public uint ProcessId { get; private set; } = eventArgs.ProcessId;
    public string ProcessName { get; private set; } = eventArgs.ProcessName;

    private readonly ConcurrentQueue<string> _messages = new();

    public void PushMessage(PipeMessage message)
    {
        if (message.Data == null)
            return;

        _messages.Enqueue(message.Data);
        while (_messages.Count > ConfigManager.MaxConsoleLines) _messages.TryDequeue(out _);
    }

    private string _filter = "";

    protected override void Render()
    {
        if (!IsShow)
            return;

        ImGui.PushID((int)ProcessId);

        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 300), new Vector2(float.MaxValue, float.MaxValue));
        ImGui.Begin($"Console: {ProcessName}[{ProcessId}]", ImGuiWindowFlags.NoSavedSettings);
        {
            // Window
            ImGui.InputTextWithHint("##Filter", "Filter", ref _filter, 1024);

            ImGui.SameLine();
            if (ImGui.Button("×"))
            {
                _filter = string.Empty;
            }

            ImGui.SameLine();
            if (ImGui.Button(Translator.GetTranslation("Hide")))
            {
                IsShow = false;
            }

            ImGui.SameLine(ImGui.GetWindowWidth() - 45);
            // ImGui.Text("Status:");
            // ImGui.SameLine();
            ImGui.TextColored(IsAlive ? Colors.StatusOnline : Colors.StatusOffline,
                Translator.GetTranslation(IsAlive ? "Online" : "Offline"));

            ImGui.BeginChild("Messages", new Vector2(0, 0),
                ImGuiChildFlags.Border | ImGuiChildFlags.NavFlattened);
            {
                // Begin Child
                foreach (var message in _messages)
                {
                    if (!_filter.IsNullOrEmpty() && !message.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var isLuaError = message.Contains("LUA ERROR stack traceback:");

                    if (isLuaError)
                        ImGui.PushStyleColor(ImGuiCol.Text, Colors.Error);
                    ImGui.TextWrapped(message);

                    if (isLuaError)
                        ImGui.PopStyleColor();
                }

                if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                    ImGui.SetScrollHereY(1);

                // if (AutoScroll)
                //     ImGui.SetScrollHereY(1);

                ImGui.EndChild();
            }

            ImGui.End();
        }

        ImGui.PopID();
    }
}