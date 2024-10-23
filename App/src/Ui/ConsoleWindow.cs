using System.Collections.Concurrent;
using System.Numerics;
using App.Communication;
using App.Util;
using ImGuiNET;

namespace App.Ui;

public class ConsoleWindow(ProcessWatcher.ProcessEventArgs eventArgs) : BaseWindow
{
    public bool IsAlive { get; set; } = true;
    public bool IsShow { get; set; } = true;

    private const int MaxMessages = 100;
    private readonly ConcurrentQueue<string> _messages = new();

    public void PushMessage(PipeMessage message)
    {
        if (message.Data == null)
            return;

        _messages.Enqueue(message.Data);
        while (_messages.Count > MaxMessages) _messages.TryDequeue(out _);
    }

    private bool AutoScroll = true;

    protected override void Render()
    {
        if (!IsShow)
            return;

        ImGui.PushID((int)eventArgs.ProcessId);

        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 300), new Vector2(float.MaxValue, float.MaxValue));
        ImGui.Begin($"Console: {eventArgs.ProcessName}[{eventArgs.ProcessId}]", ImGuiWindowFlags.NoSavedSettings);
        {
            // Window
            ImGui.Checkbox("Auto-scroll", ref AutoScroll);
            if (!IsAlive)
            {
                ImGui.SameLine(ImGui.GetWindowWidth() - 155);
                if (ImGui.Button("Hide"))
                {
                    IsShow = false;
                }
            }
          
            ImGui.SameLine(ImGui.GetWindowWidth() - 100);
            ImGui.Text("Status:");
            ImGui.SameLine();
            if (IsAlive)
                ImGui.TextColored(new Vector4(0, 1, 0, 1), "Alive");
            else
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Dead");
            
            ImGui.BeginChild("Messages", new Vector2(0, 0),
                ImGuiChildFlags.Border | ImGuiChildFlags.NavFlattened);
            {
                // Begin Child
                foreach (var message in _messages)
                {
                    var isLuaError = message.Contains("LUA ERROR stack traceback:");
                    
                    if (isLuaError)
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                    ImGui.TextWrapped(message);
                    
                    if (isLuaError)
                        ImGui.PopStyleColor();
                }

                if (AutoScroll)
                    ImGui.SetScrollHereY(1);

                ImGui.EndChild();
            }

            ImGui.End();
        }

        ImGui.PopID();
    }
}