using Godot;

namespace SaintPatrick.Entities.Humans.Human.Components;

[GlobalClass]
public sealed partial class HumanPortrait : Node
{
    /// <summary>
    /// The loaded portrait texture, or null when no portrait file exists for this human.
    /// </summary>
    public Texture2D? Value { get; private set; }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        var owner = base.GetOwner<Human>();
        var path = string.Join("/", owner.SceneFilePath.GetBaseDir(), $"{owner.Name.ToString().ToLower()}.portrait.png");
        this.Value = ResourceLoader.Exists(path)
            ? GD.Load<Texture2D>(path)
            : null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        base._ExitTree();

        this.Value = null;
    }
}