using Microsoft.Xna.Framework;

namespace DoodleJump;

public enum PlatformKind
{
    Normal,
    Moving,
    Breakable
}

public sealed class Platform
{
    public const int Width = 72;
    public const int Height = 14;

    public Vector2 Position;
    public PlatformKind Kind { get; }
    public bool Active { get; private set; } = true;

    private float _moveDirection = 1f;
    private readonly float _moveSpeed;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public Platform(Vector2 position, PlatformKind kind, Random random)
    {
        Position = position;
        Kind = kind;
        _moveDirection = random.Next(2) == 0 ? -1f : 1f;
        _moveSpeed = random.Next(90, 160);
    }

    public void Update(float dt, int screenWidth)
    {
        if (!Active || Kind != PlatformKind.Moving)
            return;

        Position.X += _moveSpeed * _moveDirection * dt;
        if (Position.X <= 0f)
        {
            Position.X = 0f;
            _moveDirection = 1f;
        }
        else if (Position.X + Width >= screenWidth)
        {
            Position.X = screenWidth - Width;
            _moveDirection = -1f;
        }
    }

    public void Break()
    {
        Active = false;
    }

    public Color Color => Kind switch
    {
        PlatformKind.Moving => new Color(120, 180, 255),
        PlatformKind.Breakable => new Color(255, 140, 90),
        _ => new Color(90, 200, 110)
    };
}
