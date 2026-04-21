using Godot;

namespace Utils;

[GlobalClass]
[Tool]
public partial class LevelBounds : Node2D
{
    private int _width = 480;
    private int _height = 270;

    [Export(PropertyHint.Range, "480,2048,32,suffix:px")]
    public int Width
    {
        get => _width;
        set
        {
            _width = value;
            QueueRedraw();
        }
    }
    [Export(PropertyHint.Range, "270,2048,32,suffix:px")]
    public int Height
    {
        get => _height;
        set
        {
            _height = value;
            QueueRedraw();
        }
    }

    public override async void _Ready()
    {
        base._Ready();
        // Handle z-index
        ZIndex = 256;

        if (Engine.IsEditorHint())
        {
            return;
        }

        // Get reference to our camera
        Camera2D? camera = null;
        while (camera is null)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            camera = GetViewport().GetCamera2D();
        }

        // Update camera limits
        camera.LimitLeft = (int)GlobalPosition.X;
        camera.LimitTop = (int)GlobalPosition.Y;
        camera.LimitRight = (int)GlobalPosition.X + Width;
        camera.LimitBottom = (int)GlobalPosition.Y + Height;
    }

    public override void _Draw()
    {
        base._Draw();
        if (Engine.IsEditorHint())
        {
            DrawRect(
                new Rect2(Vector2.Zero, new Vector2(Width, Height)),
                new Color(0f, 0.45f, 1.0f, 0.6f),
                false,
                3
            );

            DrawRect(
                new Rect2(Vector2.Zero, new Vector2(Width, Height)),
                new Color(0f, 0.75f, 1.0f),
                false,
                1
            );
        }
    }

}
