using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace App.Ui;

public class MainWindow : BaseWindow
{
    private static readonly string[] _pages = ["Main", "Settings", "About"];

    private int _currentPage;

    private void RenderPageSelector()
    {
        ImGui.BeginChild("PageSelector", new Vector2(0, 20));
        for (var i = 0; i < _pages.Length; i++)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
            if (ImGui.Selectable(_pages[i], i == _currentPage, ImGuiSelectableFlags.None,
                    new Vector2(ImGui.GetWindowWidth() / _pages.Length - 5, 0)))
            {
                _currentPage = i;
            }

            ImGui.PopStyleVar();

            if (i < _pages.Length - 1)
                ImGui.SameLine();
        }

        ImGui.EndChild();
    }

    private void RenderAboutPage()
    {
        ImGui.Text("Rendering About Page");
    }

    private void RenderSettingsPage()
    {
        ImGui.Text("Rendering Settings Page");
    }

    private void RenderMainPage()
    {
        ImGui.Text("Rendering Main Page");
    }

    private void RenderPageContent()
    {
        ImGui.BeginChild("PageContent", new Vector2(0, 0), ImGuiChildFlags.Border);
        
        switch (_currentPage)
        {
            case 0:
                RenderMainPage();
                break;
            case 1:
                RenderSettingsPage();
                break;
            case 2:
                RenderAboutPage();
                break;
        }

        ImGui.EndChild();
    }

    protected override void Render()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(200, 350), new Vector2(float.MaxValue, float.MaxValue));
        ImGui.Begin("App");

        RenderPageSelector();
        RenderPageContent();

        ImGui.End();
    }
}