using System;
using DialogueManagerRuntime;
using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class Dialogue : Node
{
    public event Action<string>? DialogueStarted;
    public event Action<string>? DialogueEnded;

    private Resource? _resource;
    private string? _currentDialogueId;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._resource = Dialogue.LoadDialogueOrNull(base.GetOwner());
        this._currentDialogueId = null;

        DialogueManager.DialogueStarted += this.OnDialogueStarted;
        DialogueManager.DialogueEnded += this.OnDialogueEnded;
    }

    private void OnDialogueStarted(Resource resource)
    {
        if (this._resource != resource)
            return;

        if (this._currentDialogueId == null)
            throw new InvalidOperationException(); // TODO:

        this.DialogueStarted?.Invoke(this._currentDialogueId);
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Play(string dialogueId)
    {
        if (this._currentDialogueId != null)
            throw new InvalidOperationException(); // TODO:

        if (this._resource == null)
            throw new InvalidOperationException(); // TODO:

        this._currentDialogueId = dialogueId;
        DialogueManager.ShowDialogueBalloon(this._resource, this._currentDialogueId);
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Stop()
    {
        if (this._resource == null)
            return;

        this.OnDialogueEnded(this._resource);
    }

    private void OnDialogueEnded(Resource resource)
    {
        if (this._resource != resource)
            return;

        if (this._currentDialogueId == null)
            return;

        this.DialogueEnded?.Invoke(this._currentDialogueId);
        this._currentDialogueId = null;
    }

    public override void _ExitTree()
    {
        DialogueManager.DialogueStarted -= this.OnDialogueStarted;
        DialogueManager.DialogueEnded -= this.OnDialogueEnded;

        this._currentDialogueId = null;
        this._resource = null;

        base._ExitTree();
    }

    private static Resource? LoadDialogueOrNull(Node owner)
    {
        var path = owner.SceneFilePath.GetBaseDir().PathJoin($"{owner.SceneFilePath.GetFile().GetBaseName()}.dialogue");

        if (!ResourceLoader.Exists(path))
            return null;

        return ResourceLoader.Load(path)?.Duplicate();
    }
}