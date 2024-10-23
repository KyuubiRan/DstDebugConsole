using System.Numerics;
using ImGuiNET;

namespace App.Ui;

public class MainWindow : BaseWindow
{
    
    private static readonly string[] Items = ["Hello, world!", "你好，世界！", "こんにちは、世界！", "안녕하세요, 세계!", "Привет, мир!", "สวัสดี, โลก!", "Chào thế giới!"];
    
    protected override void Render()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(200, 350), new Vector2(float.MaxValue, float.MaxValue));
        ImGui.Begin("App");
 
    
        foreach (var item in Items)
        {
            ImGui.Text(item);
        }
        
        ImGui.End();
    }
}