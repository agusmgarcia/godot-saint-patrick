using ECS.Utils;
using Godot;

namespace ECS.Components;

/// <summary>
/// Dialogue driven by the DialogueManager balloon UI requiring player input.
/// </summary>
public partial class InteractiveDialogue : Dialogue
{
    private Dictionary<string, int>? _tags;
    private Node? _balloon;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        DialogueManagerWrapper.GotDialogue += this.OnGotDialogue;
        DialogueManagerWrapper.PassedCue += this.OnPassedCue;
        DialogueManagerWrapper.DialogueEnded += this.OnDialogueEnded;

        this._tags = base.Resource.Get("cues")
            .AsGodotDictionary<string, string>()
            .ToDictionary(x => x.Key, x => x.Value.ToInt());

        this._balloon = null;
        this._balloon = DialogueManagerWrapper.ShowDialogueBalloon(base.Resource, "", new Godot.Collections.Array<Variant>() { base.Resource });
    }

    private void OnGotDialogue(DialogueManagerWrapper.DialogueLineWrapper line)
    {
        if (!line.ExtraGameStates.Contains(base.Resource))
            return;

        base.CurrentLine = new(line, base.Initiator, base.Targets);
    }

    private void OnPassedCue(string tag)
    {
        if (!(this._tags?.ContainsKey(tag) ?? false))
            return;

        base.PassTag(tag);
    }

    private void OnDialogueEnded(Resource resource)
    {
        if (resource != base.Resource)
            return;

        base.CurrentLine = null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._tags = null;

        this._balloon?.QueueFree();
        this._balloon = null;

        DialogueManagerWrapper.DialogueEnded -= this.OnDialogueEnded;
        DialogueManagerWrapper.PassedCue -= this.OnPassedCue;
        DialogueManagerWrapper.GotDialogue -= this.OnGotDialogue;

        base._ExitTree();
    }
}