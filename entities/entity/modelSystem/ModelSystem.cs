using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO:
/// </summary>
[GlobalClass]
public sealed partial class ModelSystem : System<Entity>
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<ModelSystem, Node3D?, Node3D?> ModelChanged
    {
        add => this._modelObservableProperty.Changed += value;
        remove => this._modelObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public Node3D? Model
    {
        get => this._modelObservableProperty.Value;
        private set => this._modelObservableProperty.Value = value;
    }

    private readonly ObservableProperty<ModelSystem, Node3D?> _modelObservableProperty;

    public ModelSystem() =>
        this._modelObservableProperty = new() { Instance = this, Value = null };

    public override void _EnterTree()
    {
        base._EnterTree();

        var model = ModelsPool.GetModel(base.Owner);
        base.AddChild(model);
        this.Model = model;

        base.Owner.HeightChanged += this.OnHeightChanged;
        this.OnHeightChanged(base.Owner, 1, base.Owner.Height);
    }

    private void OnHeightChanged(Entity entity, float prevHeight, float newHeight) =>
        this.Model?.Scale = new Vector3(1, newHeight, 1);

    public override void _ExitTree()
    {
        this.OnHeightChanged(base.Owner, base.Owner.Height, 1);
        base.Owner.HeightChanged -= this.OnHeightChanged;

        var model = this.Model;
        this.Model = null;
        base.RemoveChild(model);

        base._ExitTree();
    }

    private static class ModelsPool
    {
        private static readonly Dictionary<string, Node3D> _CACHE = [];

        public static Node3D GetModel(Entity owner)
        {
            var ownerScriptPath = ((Script)(GodotObject)owner.GetScript()).ResourcePath;

            if (ModelsPool._CACHE.TryGetValue(ownerScriptPath, out var cachedModel))
            {
                cachedModel = (Node3D)cachedModel.Duplicate();
            }
            else
            {
                cachedModel = GD.Load<PackedScene>(ownerScriptPath.PathJoin($"{ownerScriptPath.GetFile()}.fbx")).Instantiate<Node3D>();
                ModelsPool._CACHE.Add(ownerScriptPath, cachedModel);
            }

            return cachedModel;
        }
    }
}