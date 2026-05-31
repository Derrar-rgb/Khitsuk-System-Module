using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DoodleJump;

public sealed class Player
{
    public const int Width = 36;
    public const int Height = 36;

    public Vector2 Position;
    public Vector2 Velocity;

    private const float Gravity = 2150f;
    private const float JumpSpeed = -1180f;
    /// <summary>3× normal jump impulse (|v| = 3 × |JumpSpeed|).</summary>
    private const float MegaJumpSpeed = JumpSpeed * 3f;
    private const float MoveSpeed = 420f;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public void Reset(Vector2 spawn)
    {
        Position = spawn;
        Velocity = Vector2.Zero;
    }

    public void Update(float dt, KeyboardState keyboard, int screenWidth)
    {
        float direction = 0f;
        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
            direction -= 1f;
        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
            direction += 1f;

        Velocity.X = direction * MoveSpeed;
        Velocity.Y += Gravity * dt;
        Position += Velocity * dt;

        if (Position.X < -Width)
            Position.X = screenWidth;
        else if (Position.X > screenWidth)
            Position.X = -Width;
    }

    public void Bounce()
    {
        if (Velocity.Y > 0f)
            Velocity.Y = JumpSpeed;
    }

    public void MegaJump() => Velocity.Y = MegaJumpSpeed;

    public bool IsFalling => Velocity.Y > 0f;
}
