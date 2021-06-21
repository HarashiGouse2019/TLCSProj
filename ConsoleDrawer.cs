using System;

namespace TLCSProj.UI
{
    internal static class ConsoleDrawer
    {
        internal static void Draw(Label input, int x, int y, ConsoleColor color = ConsoleColor.White)
        {
            for (int row = 0; row < Console.WindowWidth; row++)
            {
                for (int col = 0; col < Console.WindowHeight; col++)
                {
                    if (row == x && col == y)
                    {
                        
                        Console.SetCursorPosition(x, y);

                        var initColor = Console.ForegroundColor;
                        Console.ForegroundColor = color;

                        Console.Write(input.Content);
                        
                        Console.ForegroundColor = initColor;
                        return;
                    }
                }
            }
        }
    }
}