using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class Coin
{
    public const int Size = 22;

    public Vector2 Position;
    public bool Collected { get; private set; }

    public float SpinPhase { get; }

    public Coin(Vector2 position, float spinPhase = 0f)
    {
        Position = position;
        SpinPhase = spinPhase;
    }

    public Rectangle Bounds => new(
        (int)Position.X,
        (int)Position.Y,
        Size,
        Size);

    public void Collect() => Collected = true;
}
