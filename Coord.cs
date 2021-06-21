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
}