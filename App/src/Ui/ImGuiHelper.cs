using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

namespace App.Ui;

public static class ImGuiHelper
{
    public static void HoveredTooltip(string text, bool sameline = true)
    {
        if (sameline)
            ImGui.SameLine();

        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public static void ClickableLink(string link, string text, bool sameline = true)
    {
        if (sameline)
            ImGui.SameLine();

        ImGui.TextColored(Colors.Link, text);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(link);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                Process.Start("explorer.exe", link);
            }
        }
    }
    
    public static void DrawLine(Vector2 start, Vector2 end, Vector4 color)
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddLine(start, end, ImGui.GetColorU32(color));
    }
}