using System.Numerics;
using System.Reflection;
using App.Communication;
using App.Util;
using FoxTail.Extensions;
using ImGuiNET;

namespace App.Ui;

public class MainWindow : BaseWindow
{
    private const string Version = "0.1.0";

    private static readonly string[] Pages =
    [
        "Main", "Settings", "About"
#if DEBUG
        , "Debug"
#endif
    ];

    private int _currentPage;

    private void RenderPageSelector()
    {
        ImGui.BeginChild("PageSelector", new Vector2(0, 20));
        for (var i = 0; i < Pages.Length; i++)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
            if (ImGui.Selectable(Translator.GetTranslation(Pages[i]), i == _currentPage, ImGuiSelectableFlags.None,
                    new Vector2(ImGui.GetWindowWidth() / Pages.Length - 5, 0)))
            {
                _currentPage = i;
            }

            ImGui.PopStyleVar();

            if (i < Pages.Length - 1)
                ImGui.SameLine();
        }

        ImGui.EndChild();
    }

    private void RenderAboutPage()
    {
        ImGui.Text(Translator.GetTranslation("Version", Version));
        ImGui.Text(Translator.GetTranslation("Author"));
        ImGuiHelper.ClickableLink("https://github.com/KyuubiRan", "KyuubiRan");
        ImGui.Text(Translator.GetTranslation("ProjectUrl"));
        ImGuiHelper.ClickableLink("https://github.com/KyuubiRan/DstDebugConsole", "Github");
    }

    private void RenderSettingsPage()
    {
        // Language
        ImGui.Combo(Translator.GetTranslation("Language"), ref ConfigManager.Language,
            Translator.LanguageText, Translator.LanguageText.Length);

        // Max Console Lines
        ImGui.DragInt(Translator.GetTranslation("MaxConsoleLines"), ref ConfigManager.MaxConsoleLines, 10, 100, 10000);

        // Window Transparency
        ImGui.DragFloat(Translator.GetTranslation("WindowTransparency"), ref ConfigManager.WindowTransparency, 0.01f,
            0.0f, 1.0f);

        // Quit Button
        var contentRegion = ImGui.GetContentRegionAvail();
        ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 30);
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1, 0, 0, 1));
        if (ImGui.Button(Translator.GetTranslation("Exit"), contentRegion with { Y = 30 }))
        {
            ConfigManager.Save();
            Environment.Exit(0);
        }

        ImGui.PopStyleColor();
    }

    private static readonly Vector4 AliveColor = new Vector4(0, 1, 0, 1);
    private static readonly Vector4 DeadColor = new Vector4(1, 0, 0, 1);

    private void RenderConsoleWindowItem(ConsoleWindow window)
    {
        ImGui.PushID((int)window.ProcessId);
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Checkbox("##Show", ref window.IsShow);

        ImGui.TableNextColumn();
        ImGui.TextColored(window.IsAlive ? AliveColor : DeadColor,
            Translator.GetTranslation(window.IsAlive ? "Alive" : "Dead"));

        ImGui.TableNextColumn();
        ImGui.Text(window.ProcessName);

        ImGui.TableNextColumn();
        ImGui.Text(window.ProcessId.ToString());

        ImGui.PopID();
    }

    private void RenderMainPage()
    {
        if (ImGui.BeginTable("ConsoleWindows", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(Translator.GetTranslation("Show"), ImGuiTableColumnFlags.WidthFixed, 10f);
            ImGui.TableSetupColumn(Translator.GetTranslation("Status"), ImGuiTableColumnFlags.WidthFixed, 10f);
            ImGui.TableSetupColumn(Translator.GetTranslation("ProcessName"), ImGuiTableColumnFlags.WidthStretch, 50f);
            ImGui.TableSetupColumn(Translator.GetTranslation("Pid"), ImGuiTableColumnFlags.WidthFixed, 15f);
            ImGui.TableHeadersRow();

            foreach (var wind in ManagedConsole.ManagedWindows.OrderByDescending(x => x.Value.IsAlive))
            {
                RenderConsoleWindowItem(wind.Value);
            }

            ImGui.EndTable();
        }
    }

#if DEBUG
    private void RenderDebugPage()
    {
        if (ImGui.Button("Alloc Console"))
        {
            ManagedConsole.CreateDebugWindow(Random.Shared.NextSingle() < 0.5);
        }

        ImGui.SameLine();
        if (ImGui.Button("Alloc Console (Dead)"))
        {
            ManagedConsole.CreateDebugWindow(Random.Shared.NextSingle() < 0.5)
                .Also(x => x.IsAlive = false);
        }

       
        if (ImGui.Button("Push Messages"))
        {
            foreach (var (_, window) in ManagedConsole.ManagedWindows)
            {
                window.PushMessage(new PipeMessage
                {
                    Data = "Test Message",
                    MessageType = PipeMessage.Type.Message
                }.Also(x => x.Size = (uint)(x.Data?.Length ?? 0)));
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Push Lua error"))
        {
            const string errmesg = """
                                   [00:00:00]: [string "../mods/workshop-3347362893/scripts/modmain..."]:1: attempt to index local 'huohuo_is_cute' (a nil value)
                                   LUA ERROR stack traceback:
                                   ../mods/workshop-3347362893/scripts/modmain/huohuo.lua:1 in (field) SayHuohuoIscute (Lua) <19-90
                                   >
                                      inst = 123456 - huohuo (valid:true)
                                      noconsume = false
                                      health = table: 000000007FFFFFFF
                                      now = 233.33333333333
                                   """;
            foreach (var (_, window) in ManagedConsole.ManagedWindows)
            {
                window.PushMessage(new PipeMessage
                {
                    Data = errmesg,
                    MessageType = PipeMessage.Type.Log
                }.Also(x => x.Size = (uint)(x.Data?.Length ?? 0)));
            }
        }
    }

#endif


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
#if DEBUG
            case 3:
                RenderDebugPage();
                break;
#endif
        }

        ImGui.EndChild();
    }

    protected override void Render()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 350), new Vector2(float.MaxValue, float.MaxValue));
        ImGui.Begin("App");

        RenderPageSelector();
        RenderPageContent();

        ImGui.End();
    }
}