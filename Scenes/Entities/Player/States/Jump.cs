using Godot;
using Utils;

namespace Scenes.Entities.Player.States;

public partial class Jump : PlayerState
{
    #region Exported Properties
    [Export] public float JumpVelocity { get; private set; } = 450f;
    #endregion

    public override void Enter()
    {
        base.Enter();
        Player.PlayAnimation(GameConstants.Player.Animations.Jump);
        Player.PauseAnimation();

        Player.Velocity = Player.Velocity with { Y = -JumpVelocity };

        _ = CheckBufferJump();
    }

    public override PlayerState? HandleInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(GameConstants.Player.Input.Jump))
        {
            Player.Velocity = Player.Velocity with { Y = Player.Velocity.Y * 0.5f };
            return GetState<Fall>();
        }

        return GetState<Jump>();
    }

    public override PlayerState? Process(double delta)
    {
        SetJumpFrame();
        return GetState<Jump>();
    }

    public override PlayerState? PhysicsProcess(double delta)
    {
        if (Player.IsOnFloor())
        {
            return GetState<Idle>();
        }

        if (Player.Velocity.Y > 0f)
        {
            return GetState<Fall>();
        }

        Player.Velocity = Player.Velocity with { X = Player.Direction.X * Player.MoveSpeed };

        return GetState<Jump>();
    }


    private async Task CheckBufferJump()
    {
        if (Player.PreviousState is Fall && !Input.IsActionPressed(GameConstants.Player.Input.Jump))
        {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

            if (Player.CurrentState != this)
                return;

            Player.Velocity = Player.Velocity with { Y = Player.Velocity.Y * 0.5f };
            Player.ChangeState(Player.GetState<Fall>());
        }
    }

    private void SetJumpFrame()
    {
        float frame = Mathf.Remap(
            Player.Velocity.Y,
            -JumpVelocity,
            0f,
            0f,
            0.5f
        );

        Player.SeekAnimation(frame);
    }
}
