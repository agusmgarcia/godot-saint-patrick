using System;
using DialogueManagerRuntime;
using Godot;

namespace SaintPatrick;

// <==================== DIALOGUES CONTROLLER ====================> //
partial class Human
{
    private readonly DialoguesController _dialoguesController = new();

    private sealed partial class DialoguesController : Node3D
    {
        public event Action<string, Human?>? DialogueStarted;
        public event Action<string, Human?>? DialogueEnded;

        private bool _beingNotified;
        private Human? _talker;
        private Resource? _resource;
        private string? _currentDialogueId;
        private Human? _currentListener;

        public override void _EnterTree()
        {
            base._EnterTree();

            this._beingNotified = false;
            this._talker = base.GetParent<Human>();
            this._resource = ResourceLoader.Load(this._talker.SceneFilePath.GetBaseDir().PathJoin($"{this._talker.SceneFilePath.GetFile().GetBaseName()}.dialogue")).Duplicate();
            this._currentDialogueId = null;
            this._currentListener = null;

            DialogueManager.DialogueStarted += this.OnDialogueStarted;
            DialogueManager.DialogueEnded += this.OnDialogueEnded;
        }

        private void OnDialogueStarted(Resource resource)
        {
            if (this._resource != resource)
                return;

            if (this._beingNotified)
                throw new InvalidOperationException();

            if (this._currentDialogueId == null)
                throw new InvalidOperationException();

            var currentDialogueId = this._currentDialogueId;
            var currentListener = this._currentListener;

            this._beingNotified = true;
            this.DialogueStarted?.Invoke(currentDialogueId, currentListener);
            this._beingNotified = false;
        }

        public void Play(string dialogueId, Human? listener)
        {
            if (this._currentDialogueId != null)
                throw new InvalidOperationException();

            if (this._resource == null)
                throw new ArgumentNullException(nameof(this._resource));

            if (this._talker == null)
                throw new ArgumentNullException(nameof(this._talker));

            this._currentDialogueId = dialogueId;
            this._currentListener = listener;

            DialogueManager.ShowDialogueBalloon(this._resource, this._currentDialogueId, [this._talker]);
        }

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

            if (this._beingNotified)
                throw new InvalidOperationException();

            if (this._currentDialogueId == null)
                throw new InvalidOperationException();

            var currentDialogueId = this._currentDialogueId;
            var currentListener = this._currentListener;

            this._beingNotified = true;
            this.DialogueEnded?.Invoke(currentDialogueId, currentListener);
            this._beingNotified = false;

            this._currentListener = null;
            this._currentDialogueId = null;
        }

        public override void _ExitTree()
        {
            DialogueManager.DialogueStarted -= this.OnDialogueStarted;
            DialogueManager.DialogueEnded -= this.OnDialogueEnded;

            this._currentListener = null;
            this._currentDialogueId = null;
            this._resource = null;
            this._talker = null;
            this._beingNotified = false;

            base._ExitTree();
        }
    }
}