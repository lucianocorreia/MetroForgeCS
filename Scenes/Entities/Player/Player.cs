using Godot;
using Scenes.Entities.Player.States;
using Utils;

namespace Scenes.Entities.Player;

public partial class Player : CharacterBody2D
{
    #region Constants

    private const float Gravity = 980f;

    #endregion

    #region Exported Properties

    [Export] public float MoveSpeed { get; private set; } = 200f;
    [Export] public float MaxFallVelocity { get; private set; } = 600f;

    #endregion

    #region State Machine

    protected List<PlayerState> States { get; set; } = [];
    public PlayerState? CurrentState =>
        States.Count > 0 ? States[0] : null;
    public PlayerState? PreviousState =>
        States.Count > 1 ? States[1] : null;

    #endregion

    #region Runtime Properties

    public Vector2 Direction { get; private set; }
    public float GravityMultiplier { get; set; } = 1f;

    #endregion

    #region Cached Nodes

    public CollisionShape2D CollisionStand = default!;
    public CollisionShape2D CollisionCrouch = default!;
    public ShapeCast2D OneWayPlatformShapeCast = default!;

    private Sprite2D _sprite = default!;
    private AnimationPlayer _animationPlayer = default!;

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        base._Ready();

        CollisionStand = GetNode<CollisionShape2D>("CollisionStand");
        CollisionCrouch = GetNode<CollisionShape2D>("CollisionCrouch");
        OneWayPlatformShapeCast = GetNode<ShapeCast2D>("OneWayPlatformShapeCast");

        _sprite = GetNode<Sprite2D>("Sprite2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        InitializeStates();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        ChangeState(CurrentState?.HandleInput(@event));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateDirection();
        ChangeState(CurrentState?.Process(delta));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Velocity = new Vector2(
            Velocity.X,
            (float)Mathf.Clamp(
                Velocity.Y + Gravity * delta * GravityMultiplier,
                -1000f,
                MaxFallVelocity
            )
        );

        MoveAndSlide();

        ChangeState(CurrentState?.PhysicsProcess(delta));
    }

    #endregion

    #region State Management

    public void ChangeState(PlayerState? newState)
    {
        if (newState is null)
            return;

        if (newState == CurrentState)
            return;

        CurrentState?.Exit();

        States.Insert(0, newState);

        CurrentState?.Enter();

        while (States.Count > 3)
        {
            States.RemoveAt(States.Count - 1);
        }
    }

    private void InitializeStates()
    {
        States = [];

        foreach (var state in GetNode<Node>("States").GetChildren())
        {
            if (state is PlayerState playerState)
            {
                playerState.Player = this;

                States.Add(playerState);

                playerState.Init();
            }
        }

        if (States.Count == 0)
            return;

        ChangeState(CurrentState!);
        CurrentState?.Enter();
    }

    public T GetState<T>() where T : PlayerState
    {
        foreach (var state in GetNode<Node>("States").GetChildren())
        {
            if (state is T typed)
                return typed;
        }

        throw new Exception($"{typeof(T).Name} not found.");
    }

    #endregion

    #region Animation

    public void PlayAnimation(string animationName)
    {
        if (_animationPlayer.HasAnimation(animationName))
        {
            _animationPlayer.Play(animationName);
            return;
        }

        GD.PrintErr($"Animation '{animationName}' not found in AnimationPlayer.");
    }

    public void SeekAnimation(float position)
    {
        _animationPlayer.Seek(position, true);
    }

    public void PauseAnimation()
    {
        _animationPlayer.Pause();
    }

    #endregion

    #region Movement

    private void UpdateDirection()
    {
        var previousDirection = Direction;

        var xAxis = Input.GetAxis(
            GameConstants.Player.Input.MoveLeft,
            GameConstants.Player.Input.MoveRight
        );

        var yAxis = Input.GetAxis(
            GameConstants.Player.Input.MoveUp,
            GameConstants.Player.Input.MoveDown
        );

        Direction = new Vector2(xAxis, yAxis);

        if (previousDirection.X == Direction.X)
            return;

        if (Direction.X < 0)
        {
            _sprite.FlipH = true;
        }
        else if (Direction.X > 0)
        {
            _sprite.FlipH = false;
        }
    }

    #endregion
}
