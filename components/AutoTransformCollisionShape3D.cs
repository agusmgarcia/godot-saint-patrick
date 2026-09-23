using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class AutoTransformCollisionShape3D : CollisionShape3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public new Node? Owner { get; private set; }

    private Height? _height;

    private float _initialHeight;
    private float _initialRadius;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.Owner = this.FindOwner<Node>();

        this.TrackNodes<Height>(this.OnHeightTracked, this.OnHeightUntracked, unique: true);

        if (base.Shape is CapsuleShape3D capsule)
        {
            this._initialHeight = capsule.Height;
            this._initialRadius = capsule.Radius;
        }
        else
        {
            throw new NotImplementedException($"Shape of type '{base.Shape.GetType().Name}' is not supported");
        }
    }

    private void OnHeightTracked(Height height)
    {
        this._height = height;

        if (base.Shape is CapsuleShape3D capsule)
        {
            capsule.Height = this._height.Value;
            capsule.Radius = this._initialRadius * (this._height.Value / this._initialHeight);
        }
        else
        {
            throw new NotImplementedException($"Shape of type '{base.Shape.GetType().Name}' is not supported");
        }

        base.Position = new Vector3(0f, this._height.Value / 2f, 0f);
    }

    private void OnHeightUntracked(Height height)
    {
        base.Position = Vector3.Zero;

        if (base.Shape is CapsuleShape3D capsule)
        {
            capsule.Radius = this._initialRadius;
            capsule.Height = this._initialHeight;
        }
        else
        {
            throw new NotImplementedException($"Shape of type '{base.Shape.GetType().Name}' is not supported");
        }

        this._height = null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        if (base.Shape is CapsuleShape3D capsule)
        {
            this._initialHeight = capsule.Height;
            this._initialRadius = capsule.Radius;
        }
        else
        {
            throw new NotImplementedException($"Shape of type '{base.Shape.GetType().Name}' is not supported");
        }

        this.Owner = null;

        base._ExitTree();
    }
}
