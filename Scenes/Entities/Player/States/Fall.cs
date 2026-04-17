using Godot;
using Utils;

namespace Scenes.Entities.Player.States;

public partial class Fall : PlayerState
{
    [Export] public float FallGravityMultiplier { get; private set; } = 1.165f;
    [Export] public float CoyoteTime { get; private set; } = 0.4f;
    [Export] public float JumpBufferTime { get; private set; } = 0.2f;

    private float CoyoteTimer = 0f;
    private float JumpBufferTimer = 0f;

    public override void Enter()
    {
        base.Enter();
        Player.PlayAnimation(GameConstants.Player.Animations.Jump);
        Player.PauseAnimation();
        Player.GravityMultiplier = FallGravityMultiplier;
        if (Player.PreviousState is Jump)
        {
            CoyoteTimer = 0f;
        }
        else
        {
            CoyoteTimer = CoyoteTime;
        }

    }

    public override void Exit()
    {
        base.Exit();
        Player.GravityMultiplier = 1f;
        JumpBufferTimer = 0f;
    }

    public override PlayerState? HandleInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(GameConstants.Player.Input.Jump))
        {
            if (CoyoteTimer > 0f)
            {
                return GetState<Jump>();
            }

            JumpBufferTimer = JumpBufferTime;
        }

        return GetState<Fall>();
    }

    public override PlayerState? Process(double delta)
    {
        CoyoteTimer -= (float)delta;
        JumpBufferTimer -= (float)delta;
        SetJumpFrame();
        return GetState<Fall>();
    }

    public override PlayerState? PhysicsProcess(double delta)
    {
        if (Player.IsOnFloor())
        {
            if (JumpBufferTimer > 0f)
            {
                return GetState<Jump>();
            }

            return GetState<Idle>();
        }

        Player.Velocity = Player.Velocity with { X = Player.Direction.X * Player.MoveSpeed };

        return GetState<Fall>();
    }

    private void SetJumpFrame()
    {
        float frame = Mathf.Remap(
            Player.Velocity.Y,
            0f,
            Player.MaxFallVelocity,
            0.5f,
            1.0f
        );

        Player.SeekAnimation(frame);
    }
}
