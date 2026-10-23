using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class ModelHandler : Node3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<Node3D?>? ModelChanged;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Node3D? Model =>
        this._models.GetValueOrDefault(this.ModelType);

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public string ModelType
    {
        get;
        protected set
        {
            field = value;
            this.ModelChanged?.Invoke(this.Model);
        }
    } = string.Empty;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Position", "Position")]
    [Export]
    public bool PositionDisabled { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Height", "Height")]
    [Export]
    public bool HeightDisabled { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Height", "Height")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m")]
    public float HeightDefaultValue { get; private set; }

    private readonly Dictionary<string, Node3D?> _models = new()
    {
        [string.Empty] = null,
    };

    private Height? _height;

    protected ModelHandler()
    {
        this.TrackSiblings<Height>(
            this.OnHeightTracked,
            this.OnHeightUntracked,
            unique: true);

        this.TrackChildren<Node3D>(
            this.OnModelTracked,
            this.OnModelUntracked,
            direct: true);
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        this.ModelChanged += this.OnModelChanged;
        this.OnModelChanged(this.Model);
    }

    private void OnHeightTracked(Height height)
    {
        this._height = height;

        if (!this.PositionDisabled)
            base.Position = new Vector3(0f, -this._height.Value / 2f, 0f);

        if (!this.HeightDisabled)
            base.Scale = Vector3.One * (this._height.Value / this.HeightDefaultValue);
    }

    private void OnModelTracked(Node3D model)
    {
        var modelType = ModelHandler.GetModelType(model);
        this._models.Add(modelType, model);
        this.ModelType = modelType;
    }

    private static string GetModelType(Node3D model) =>
        model.Name.ToString().Replace("Model", "");

    private void OnModelChanged(Node3D? model)
    {
        foreach (var cachedModel in this._models.Values)
            cachedModel?.Visible = cachedModel == model;
    }

    private void OnModelUntracked(Node3D model)
    {
        var modelType = ModelHandler.GetModelType(model);
        this.ModelType = string.Empty;
        this._models.Remove(modelType);
    }

    private void OnHeightUntracked(Height height)
    {
        if (!this.HeightDisabled)
            base.Scale = Vector3.One;

        if (!this.PositionDisabled)
            base.Position = Vector3.Zero;

        this._height = null;
    }

    public override void _ExitTree()
    {
        this.OnModelChanged(this.Model);
        this.ModelChanged -= this.OnModelChanged;

        base._ExitTree();
    }
}
