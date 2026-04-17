using System.Reflection.Metadata;
using Godot;
using Utils;

namespace Scenes.Entities.Player.States;

public partial class Idle : PlayerState
{
    public override void Enter()
    {
        base.Enter();
        Player.PlayAnimation(GameConstants.Player.Animations.Idle);
    }

    public override PlayerState? HandleInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(GameConstants.Player.Input.Jump))
        {
            return GetState<Jump>();
        }

        return GetState<Idle>();
    }

    public override PlayerState? Process(double delta)
    {
        if (Player.Direction.X != 0)
        {
            return GetState<Run>();
        }

        if (Player.Direction.Y > 0)
        {
            return GetState<Crouch>();
        }

        return GetState<Idle>();
    }

    public override PlayerState? PhysicsProcess(double delta)
    {
        Player.Velocity = Player.Velocity with { X = 0f };

        if (Player.IsOnFloor() == false)
        {
            return GetState<Fall>();
        }

        return GetState<Idle>();
    }


}
