using System;
using MediaLib;

class Program
{
    public static void DrawButton(int x, int y, int width, int height)
    {
        Window.SetLineColor(100, 100, 200);
        Window.SetLineThickness(5);
        Window.SetFillColor(100, 100, 200);
        Window.DrawText("Hello world! This is a test.");
        while (true)
        {
            int[] pos = Window.GetMouseMoveLocation();
            Window.DrawCircle(
                x: pos[0], 
                y: pos[1],
                radius: 10,
                numSides: 0,
                centerOrigin: true
            );
        }
    }

    public static void Main(string[] args)
    {
        Window.Create();

        DrawButton(100, 100, 400, 100);


        Window.WaitUntilClose();
    }
}
