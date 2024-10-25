using System.Numerics;
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

        ImGui.NewLine();
        ImGui.Text(Translator.GetTranslation("FriendlyLinks") + " - " + Translator.GetTranslation("AuthorsMods"));
        ImGuiHelper.ClickableLink("https://steamcommunity.com/sharedfiles/filedetails/?id=3347362893",
            Translator.GetTranslation("Huohuo"), false);
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
        ImGui.PushStyleColor(ImGuiCol.Button, Colors.Error);
        if (ImGui.Button(Translator.GetTranslation("Exit"), contentRegion with { Y = 30 }))
        {
            ConfigManager.Save();
            Environment.Exit(0);
        }

        ImGui.PopStyleColor();
    }

    private void RenderConsoleWindowItem(ConsoleWindow window)
    {
        ImGui.PushID((int)window.ProcessId);
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Checkbox("##Show", ref window.IsShow);

        ImGui.TableNextColumn();
        ImGui.TextColored(window.IsAlive ? Colors.StatusOnline : Colors.StatusOffline,
            Translator.GetTranslation(window.IsAlive ? "Online" : "Offline"));

        ImGui.TableNextColumn();
        ImGui.Text(window.ProcessName);

        ImGui.TableNextColumn();
        ImGui.Text(window.ProcessId.ToString());

        ImGui.PopID();
    }

    private bool _showOnline = true;
    private bool _showOffline = true;

    private void RenderMainPage()
    {
        ImGui.Checkbox(Translator.GetTranslation("ShowOnline"), ref _showOnline);
        ImGui.SameLine();
        ImGui.Checkbox(Translator.GetTranslation("ShowOffline"), ref _showOffline);
        var txt = Translator.GetTranslation("ClearOffline");
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(txt).X - 25);
        if (ImGui.Button(txt))
            ManagedConsole.DeleteOfflineWindows();
        ImGuiHelper.HoveredTooltip(Translator.GetTranslation("ClearOfflineDesc"));


        if (ImGui.BeginTable("ConsoleWindows", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(Translator.GetTranslation("Show"), ImGuiTableColumnFlags.None, 5f);
            ImGui.TableSetupColumn(Translator.GetTranslation("Status"), ImGuiTableColumnFlags.None, 5f);
            ImGui.TableSetupColumn(Translator.GetTranslation("ProcessName"), ImGuiTableColumnFlags.None, 50f);
            ImGui.TableSetupColumn(Translator.GetTranslation("Pid"), ImGuiTableColumnFlags.None, 10f);
            ImGui.TableHeadersRow();

            var filter = ManagedConsole.ManagedWindows.Values;
            if (!_showOnline)
                filter = filter.Where(x => !x.IsAlive);
            if (!_showOffline)
                filter = filter.Where(x => x.IsAlive);

            foreach (var wind in filter.OrderByDescending(x => x.IsAlive))
            {
                RenderConsoleWindowItem(wind);
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