using Godot;
using Utils;

namespace Scenes.Entities.Player.States;

public partial class Crouch : PlayerState
{

    public override void Enter()
    {
        base.Enter();
        Player.PlayAnimation(GameConstants.Player.Animations.Crouch);
        Player.CollisionStand.Disabled = true;
        Player.CollisionCrouch.Disabled = false;
    }

    public override void Exit()
    {
        base.Exit();
        Player.CollisionStand.Disabled = false;
        Player.CollisionCrouch.Disabled = true;
    }

    public override PlayerState? HandleInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(GameConstants.Player.Input.Jump))
        {
            Player.OneWayPlatformShapeCast.ForceShapecastUpdate();
            if (Player.OneWayPlatformShapeCast.IsColliding())
            {
                Player.Position = Player.Position with { Y = Player.Position.Y + 4f };
                return GetState<Fall>();
            }

            return GetState<Jump>();
        }

        return GetState<Crouch>();
    }

    public override PlayerState? Process(double delta)
    {
        if (Player.Direction.Y <= 0)
        {
            return GetState<Idle>();
        }

        return GetState<Crouch>();
    }

    public override PlayerState? PhysicsProcess(double delta)
    {
        Player.Velocity = Player.Velocity with { X = 0f };

        return GetState<Crouch>();
    }
}
