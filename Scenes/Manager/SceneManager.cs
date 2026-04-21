using System;
using System.Reflection.Metadata.Ecma335;
using Godot;

public partial class SceneManager : CanvasLayer
{
    [Signal]
    public delegate void LoadSceneStartedEventHandler();

    [Signal]
    public delegate void NewSceneReadyEventHandler(string targetName, Vector2 offset);

    [Signal]
    public delegate void LoadSceneFinishedEventHandler();

    public static SceneManager? Instance
    {
        get; private set;
    }

    private Control _fade = default!;

    public override async void _Ready()
    {
        base._Ready();
        _fade = GetNode<Control>("Fade");
        _fade.Visible = false;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        EmitSignal(SignalName.LoadSceneFinished);
    }

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public async Task TransitionToScene(string scenePath, string targetAreaName, Vector2 playerOffset, string direction = "left")
    {
        var fadePosition = GetFadePosition(direction);
        _fade.Visible = true;
        GetTree().Paused = true;

        EmitSignal(SignalName.LoadSceneStarted);

        // Fade scene out
        await FadeTween(fadePosition, Vector2.Zero, 0.1f);

        var err = GetTree().ChangeSceneToFile(scenePath);

        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load scene: {scenePath}");
            return;
        }

        // await ToSignal(GetTree(), SceneTree.SignalName.SceneChanged);

        // Fade new scene in
        await FadeTween(Vector2.Zero, -fadePosition, 0.1f);

        GetTree().Paused = false;
        _fade.Visible = false;

        EmitSignal(SignalName.NewSceneReady, targetAreaName, playerOffset);

        EmitSignal(SignalName.LoadSceneFinished);
    }

    private Vector2 GetFadePosition(string direction)
    {
        var pos = new Vector2(1152 * 2, 648 * 2);

        switch (direction)
        {
            case "left":
                pos *= new Vector2(-1, 0);
                break;
            case "right":
                pos *= new Vector2(1, 0);
                break;
            case "up":
                pos *= new Vector2(0, -1);
                break;
            case "down":
                pos *= new Vector2(0, 1);
                break;
            default:
                GD.PrintErr($"Invalid direction: {direction}");
                break;
        }

        return pos;
    }

    private async Task FadeTween(Vector2 from, Vector2 to, float duration)
    {
        _fade.Position = from;

        var tween = CreateTween();
        tween.TweenProperty(_fade, "position", to, duration).From(from);

        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
