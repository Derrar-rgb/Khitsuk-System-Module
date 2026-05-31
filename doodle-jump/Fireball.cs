using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class Fireball
{
    public const int Size = 10;

    public Vector2 Position;
    public Vector2 Velocity;
    public bool Active { get; private set; } = true;

    public Rectangle Bounds => new(
        (int)Position.X,
        (int)Position.Y,
        Size,
        Size);

    public void Deactivate() => Active = false;
}
