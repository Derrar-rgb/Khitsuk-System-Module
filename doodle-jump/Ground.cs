using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class Ground
{
    public const int Height = 88;

    public float TopY { get; private set; }
    public int Width { get; private set; }

    public void Reset(int screenWidth, int screenHeight)
    {
        Width = screenWidth;
        TopY = screenHeight - Height;
    }

    public Rectangle Bounds => new(0, (int)TopY, Width, Height);

    public void Scroll(float delta) => TopY += delta;
}
