using System;
using Godot;
using Utils;

namespace Scenes.Levels.Transition;

[Tool]
public partial class LevelTransition : Node2D
{
    private int _size = 2;
    private LevelTransitionLocation _location = LevelTransitionLocation.Left;

    [Export(PropertyHint.Range, "1,12,1,or_greater")]
    public int Size
    {
        get => _size;
        set
        {
            _size = value;
            ApplyAreaSettings();
        }
    }

    [Export]
    public LevelTransitionLocation Location
    {
        get => _location; set
        {
            _location = value;
            ApplyAreaSettings();

        }
    }
    [Export(PropertyHint.File, "*.tscn")]
    public string TargetLevel { get; private set; } = string.Empty;

    [Export]
    public string TargetAreaName { get; private set; } = "LevelTransition";

    private Area2D _area = default!;

    public override void _Ready()
    {
        base._Ready();
        _area = GetNode<Area2D>("Area2D");
        ApplyAreaSettings();

        if (Engine.IsEditorHint())
        {
            return;
        }

        SceneManager.Instance!.NewSceneReady += OnNewSceneReady;
        SceneManager.Instance!.LoadSceneFinished += OnLoadSceneFinished;

    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (Engine.IsEditorHint())
        {
            return;
        }

        SceneManager.Instance!.NewSceneReady -= OnNewSceneReady;
        SceneManager.Instance!.LoadSceneFinished -= OnLoadSceneFinished;

        _area.BodyEntered -= OnBodyEntered;
    }

    private void OnNewSceneReady(string targetName, Vector2 offset)
    {
        if (targetName == Name)
        {
            var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            if (player is not null)
            {
                player.GlobalPosition = GlobalPosition + offset;
                // player.GlobalPosition = GlobalPosition + new Vector2(160, 0);
            }

        }
    }

    private async void OnLoadSceneFinished()
    {
        _area.Monitoring = false;
        _area.BodyEntered += OnBodyEntered;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _area.Monitoring = true;
    }

    private void OnBodyEntered(Node2D body)
    {
        CallDeferred(nameof(StartTransition), body);
    }

    private async void StartTransition(Node2D body)
    {
        await SceneManager.Instance!.TransitionToScene(
            TargetLevel,
            TargetAreaName,
            GetPlayerOffset(body),
            GetTransitionDirection()
        );
    }

    private string GetTransitionDirection()
    {
        return Location switch
        {
            LevelTransitionLocation.Left => "left",
            LevelTransitionLocation.Right => "right",
            LevelTransitionLocation.Top => "up",
            LevelTransitionLocation.Bottom => "down",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ApplyAreaSettings()
    {
        if (_area is null)
        {
            _area = GetNodeOrNull<Area2D>("Area2D");
        }

        if (_area is null)
        {
            return;
        }

        if (Location == LevelTransitionLocation.Left || Location == LevelTransitionLocation.Right)
        {
            _area.Scale = _area.Scale with
            {
                Y = Size
            };

            if (Location == LevelTransitionLocation.Left)
            {
                _area.Scale = _area.Scale with
                {
                    X = -1
                };
            }
            else
            {
                _area.Scale = _area.Scale with
                {
                    X = 1
                };
            }
        }
        else
        {
            _area.Scale = _area.Scale with
            {
                X = Size
            };

            if (Location == LevelTransitionLocation.Top)
            {
                _area.Scale = _area.Scale with
                {
                    Y = 1
                };
            }
            else
            {
                _area.Scale = _area.Scale with
                {
                    Y = -1
                };
            }
        }

    }

    public Vector2 GetPlayerOffset(Node2D player)
    {
        var offset = Vector2.Zero;
        var playerPosition = player.GlobalPosition;

        if (_location == LevelTransitionLocation.Left || _location == LevelTransitionLocation.Right)
        {
            offset.Y = playerPosition.Y - GlobalPosition.Y;
            if (_location == LevelTransitionLocation.Left)
            {
                offset.X = -48;
            }
            else
            {
                offset.X = 48;
            }
        }
        else
        {
            offset.X = playerPosition.X - GlobalPosition.X;
            if (_location == LevelTransitionLocation.Top)
            {
                GD.Print("Top");
                offset.Y = 48;
            }
            else
            {
                GD.Print(playerPosition);
                GD.Print("Bottom");
                offset.Y = 160;
            }
        }

        return offset;
    }
}
