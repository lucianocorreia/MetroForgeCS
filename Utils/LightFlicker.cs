using System.Threading.Tasks;
using Godot;

namespace Utils;

public partial class LightFlicker : PointLight2D
{
    [Export]
    public float FlickerIntensity { get; private set; } = 0.1f;

    [Export]
    public float FlickerFrequency { get; private set; } = 0.2f;

    public float OgEnergy { get; set; } = 1.0f;

    public override void _Ready()
    {
        base._Ready();
        OgEnergy = Energy;
        _ = Flicker();
    }

    private async Task Flicker()
    {
        float newValue =
            (float)GD.RandRange(-1.0, 1.0) * FlickerIntensity;

        Energy = OgEnergy + newValue;

        float waitTime =
            FlickerFrequency +
            (float)GD.RandRange(
                FlickerFrequency * -0.3f,
                FlickerFrequency * 0.3f
            );

        await ToSignal(
            GetTree().CreateTimer(waitTime),
            SceneTreeTimer.SignalName.Timeout
        );

        await Flicker();
    }
}
