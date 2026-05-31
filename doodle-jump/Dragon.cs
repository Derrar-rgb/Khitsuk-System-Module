using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class Dragon
{
    public const int Width = 54;
    public const int Height = 38;

    public Vector2 Position;
    public bool Active { get; private set; } = true;

    private float _shootTimer;
    private float _patrolDir = 1f;
    private readonly float _patrolSpeed;
    private readonly float _shootInterval;

    public Dragon(Vector2 position, Random random)
    {
        Position = position;
        _patrolDir = random.Next(2) == 0 ? -1f : 1f;
        _patrolSpeed = random.Next(55, 95);
        _shootInterval = random.Next(14, 22) / 10f;
        _shootTimer = random.Next(4, 12) / 10f;
    }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public bool FacingRight => _patrolDir > 0f;

    public Rectangle StompSurface => new(
        (int)Position.X + 6,
        (int)Position.Y,
        Width - 12,
        10);

    public void Update(float dt, int screenWidth, IList<Fireball> fireballs, Random random)
    {
        if (!Active)
            return;

        Position.X += _patrolSpeed * _patrolDir * dt;
        if (Position.X <= 4f)
        {
            Position.X = 4f;
            _patrolDir = 1f;
        }
        else if (Position.X + Width >= screenWidth - 4f)
        {
            Position.X = screenWidth - Width - 4f;
            _patrolDir = -1f;
        }

        _shootTimer -= dt;
        if (_shootTimer > 0f)
            return;

        _shootTimer = _shootInterval + random.Next(0, 8) / 10f;
        var mouthX = Position.X + (_patrolDir > 0f ? Width - 8f : 8f);
        var mouthY = Position.Y + 16f;
        fireballs.Add(new Fireball
        {
            Position = new Vector2(mouthX - Fireball.Size / 2f, mouthY),
            Velocity = new Vector2(_patrolDir * 180f, 260f)
        });
    }

    public void Kill() => Active = false;
}
