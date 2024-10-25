using System.Numerics;

namespace App.Ui;

public static class Colors
{
    public static readonly Vector4 StatusOnline = new(0, 1, 0, 1);
    public static readonly Vector4 StatusOffline = new(1, 0, 0, 1);
    public static readonly Vector4 Link = new(0, 0.5f, 1, 1);
    public static readonly Vector4 Error = new(1, 0, 0, 1);
}