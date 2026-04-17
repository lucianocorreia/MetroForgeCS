using Godot;
using Utils;

namespace Scenes.Entities.Player.States;

public partial class Run : PlayerState
{
    public override void Enter()
    {
        base.Enter();
        Player.PlayAnimation(GameConstants.Player.Animations.Run);
    }

    public override PlayerState? HandleInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(GameConstants.Player.Input.Jump))
        {
            return GetState<Jump>();
        }

        return GetState<Run>();
    }

    public override PlayerState? Process(double delta)
    {
        if (Player.Direction.X == 0)
        {
            return GetState<Idle>();
        }

        if (Player.Direction.Y > 0)
        {
            return GetState<Crouch>();
        }

        return GetState<Run>();
    }

    public override PlayerState? PhysicsProcess(double delta)
    {
        Player.Velocity = Player.Velocity with { X = Player.MoveSpeed * Player.Direction.X };

        if (Player.IsOnFloor() == false)
        {
            return GetState<Fall>();
        }

        return GetState<Run>();
    }

}
