using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using App.Util;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace App.Ui;

public class MainOverlay : Overlay
{
    public delegate void RenderDelegate();

    public RenderDelegate? OnRender;

    private static (int, int) ScreenResolution
    {
        get
        {
            DEVMODEW devmode = default;
#pragma warning disable CA1416
            PInvoke.EnumDisplaySettings(null, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref devmode);
#pragma warning restore CA1416
            return ((int)devmode.dmPelsWidth, (int)devmode.dmPelsHeight);
        }
    }

    public static readonly MainOverlay Instance = new(ScreenResolution);

    private MainOverlay((int x, int y) vec2) : this(vec2.x, vec2.y)
    {
    }

    private MainOverlay(int x, int y) : base(x, y)
    {
    }

    protected override Task PostInitialized()
    {
        unsafe
        {
            ReplaceFont(config =>
            {
                var io = ImGui.GetIO();

                var builder =
                    new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
                builder.AddRanges(io.Fonts.GetGlyphRangesDefault());
                builder.AddRanges(io.Fonts.GetGlyphRangesCyrillic());
                builder.AddRanges(io.Fonts.GetGlyphRangesJapanese());
                builder.AddRanges(io.Fonts.GetGlyphRangesKorean());
                builder.AddRanges(io.Fonts.GetGlyphRangesChineseFull());
                builder.AddRanges(io.Fonts.GetGlyphRangesGreek());
                builder.AddRanges(io.Fonts.GetGlyphRangesThai());
                builder.AddRanges(io.Fonts.GetGlyphRangesVietnamese());
                builder.BuildRanges(out var ranges);
                builder.Destroy();
                
                config->GlyphRanges = (ushort*)ranges.Data;

                fixed (void* ptr = AppRes.FontFile)
                    io.Fonts.AddFontFromMemoryTTF(new IntPtr(ptr), AppRes.FontFile.Length, 16f,
                        config);
            });
        }

        return base.PostInitialized();
    }

    protected override void Render()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, ConfigManager.WindowTransparency));
        OnRender?.Invoke();
        ImGui.PopStyleColor();
    }
}