namespace App.Ui;

public abstract class BaseWindow
{
    protected BaseWindow()
    {
        MainOverlay.Instance.OnRender += Render;
    }
    
    ~BaseWindow()
    {
        MainOverlay.Instance.OnRender -= Render;
    }

    protected abstract void Render();
}