using System;

namespace TLCSProj.UI
{
    internal class Coord
    {
        internal int x, y = 0;
        internal static Coord Zero => new Coord(0, 0);
        internal Coord(int initX, int initY)
        {
            x = initX;
            y = initY;
        }
    }

    internal static class ConsoleDrawer
    {
        
        internal static Coord Draw(Label input, int x, int y)
        {
            Coord objectCoord = new Coord(x, y);
            for (int row = 0; row < Console.WindowWidth; row++)
            {
                for (int col = 0; col < Console.WindowHeight; col++)
                {
                    if (row == x && col == y)
                    {
                        
                        Console.SetCursorPosition(objectCoord.x, objectCoord.y);
                        Console.Out.Write(input.Content);
                        return objectCoord;
                    }
                }
            }

            return Coord.Zero;
        }
    }
}