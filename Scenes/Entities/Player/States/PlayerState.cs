namespace Scenes.Entities.Player.States;

using Godot;
using Scenes.Entities.Player;

public abstract partial class PlayerState : Node
{
    public Player Player { get; set; } = default!;

    public virtual void Init() { }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual PlayerState? HandleInput(InputEvent inputEvent) => null;
    public virtual PlayerState? Process(double delta) => null;
    public virtual PlayerState? PhysicsProcess(double delta) => null;

    protected T GetState<T>() where T : PlayerState
    {
        return Player.GetState<T>();
    }
}
