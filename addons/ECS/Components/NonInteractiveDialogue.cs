using ECS.Utils;
using Godot;

namespace ECS.Components;

/// <summary>
/// Dialogue that auto-advances lines on a timed delay without player input.
/// </summary>
public partial class NonInteractiveDialogue : Dialogue
{
    private double _delay = 0;
    private string _nextId = string.Empty;
    private bool _canPassTags = false;
    private Dictionary<string, int>? _tags;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        DialogueManagerWrapper.PassedCue += this.OnPassedCue;

        this._delay = 0;
        this._nextId = string.Empty;
        this._canPassTags = false;
        this._tags = base.Resource.Get("cues")
            .AsGodotDictionary<string, string>()
            .ToDictionary(x => x.Key, x => x.Value.ToInt());
    }

    private void OnPassedCue(string tag)
    {
        if (!this._canPassTags)
            return;

        if (!(this._tags?.ContainsKey(tag) ?? false))
            return;

        this.PassTag(tag);
    }

    /// <inheritdoc/>
    public override async void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (base.CurrentLine == null)
            return;

        this._delay -= delta;
        if (this._delay > 0)
            return;

        this._canPassTags = true;
        var line = await DialogueManagerWrapper.GetNextDialogueLine(base.Resource, this._nextId);
        this._canPassTags = false;

        if (!base.IsInsideTree())
            return;

        if (line == null)
        {
            base.CurrentLine = null;
            return;
        }

        base.CurrentLine = new(line.Value, base.Initiator, base.Targets);

        this._delay = (line.Value.Text.Length * 0.05f) + GD.RandRange(0.2, 0.8);
        this._nextId = (line.Value.Responses.Count > 0)
            ? line.Value.Responses.First(x => x.IsAllowed).NextId
            : line.Value.NextId;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._tags = null;
        this._canPassTags = false;
        this._nextId = string.Empty;
        this._delay = 0;

        DialogueManagerWrapper.PassedCue -= this.OnPassedCue;

        base._ExitTree();
    }
}