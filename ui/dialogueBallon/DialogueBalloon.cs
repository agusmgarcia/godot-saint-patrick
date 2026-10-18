using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using SaintPatrick.Entities.Humans.Human.Components;

namespace SaintPatrick.UI.DialogueBalloon
{
    public sealed partial class DialogueBalloon : CanvasLayer
    {
        [Export]
        public Resource? DialogueResource { get; private set; }

        [Export]
        public string StartFromLabel { get; private set; } = "";

        [Export]
        public bool AutoStart { get; private set; }

        [Export]
        public string NextAction { get; private set; } = "ui_accept";

        [Export]
        public string SkipAction { get; private set; } = "ui_cancel";

        private Control? _balloon;
        private RichTextLabel? _characterLabel;
        private DialogueLabel? _dialogueLabel;
        private DialogueResponsesMenu? _responsesMenu;
        private Polygon2D? _progress;
        private TextureRect? _portraitLeft;
        private TextureRect? _portraitRight;

        private Array<Variant> _temporaryGameStates = [];
        private bool _isWaitingForInput = false;
        private bool _willHideBalloon = false;
        private Godot.Timer _mutationCooldown = new();

        private DialogueLine? DialogueLine
        {
            get;
            set
            {
                if (value == null)
                {
                    if (Owner == null)
                        base.QueueFree();
                    else
                        base.Hide();
                    return;
                }

                field = value;
                this.ApplyDialogueLine();
            }
        }

        public override void _Ready()
        {
            this._balloon = GetNode<Control>("%Balloon");
            this._characterLabel = GetNode<RichTextLabel>("%CharacterLabel");
            this._dialogueLabel = GetNode<DialogueLabel>("%DialogueLabel");
            this._responsesMenu = GetNode<DialogueResponsesMenu>("%ResponsesMenu");
            this._progress = GetNode<Polygon2D>("%Progress");
            this._portraitLeft = GetNode<TextureRect>("%PortraitLeft");
            this._portraitRight = GetNode<TextureRect>("%PortraitRight");

            this._balloon.Hide();

            this._balloon.GuiInput += (@event) =>
            {
                if (this._dialogueLabel.IsTyping)
                {
                    bool mouseWasClicked = @event is InputEventMouseButton && @event is InputEventMouseButton { ButtonIndex: MouseButton.Left } && @event.IsPressed();
                    bool skipButtonWasPressed = @event.IsActionPressed(this.SkipAction);
                    if (mouseWasClicked || skipButtonWasPressed)
                    {
                        base.GetViewport().SetInputAsHandled();
                        this._dialogueLabel.SkipTyping();
                        return;
                    }
                }

                if (!this._isWaitingForInput)
                    return;

                if (this.DialogueLine != null && this.DialogueLine.Responses.Count > 0)
                    return;

                base.GetViewport().SetInputAsHandled();

                if (@event is InputEventMouseButton && @event.IsPressed() && @event is InputEventMouseButton { ButtonIndex: MouseButton.Left })
                    this.Next(this.DialogueLine?.NextId ?? "");
                else if (@event.IsActionPressed(this.NextAction) && GetViewport().GuiGetFocusOwner() == this._balloon)
                    this.Next(this.DialogueLine?.NextId ?? "");
            };

            if (string.IsNullOrEmpty(this._responsesMenu.NextAction))
                this._responsesMenu.NextAction = this.NextAction;

            this._responsesMenu.ResponseSelected += (response) =>
                this.Next(response.NextId);

            this._mutationCooldown.Timeout += () =>
            {
                if (this._willHideBalloon)
                {
                    this._willHideBalloon = false;
                    this._balloon.Hide();
                }
            };

            base.AddChild(this._mutationCooldown);

            DialogueManager.Mutated += OnMutated;

            if (this.AutoStart)
            {
                if (!GodotObject.IsInstanceValid(this.DialogueResource))
                    throw new System.Exception(DialogueManager.GetErrorMessage(143));

                this.Start();
            }
        }

        private async void Start(Resource? dialogueResource = null, string label = "", Array<Variant>? extraGameStates = null)
        {
            this._temporaryGameStates = new Array<Variant> { this } + (extraGameStates ?? []);
            this._isWaitingForInput = false;

            if (dialogueResource != null && GodotObject.IsInstanceValid(dialogueResource))
                this.DialogueResource = dialogueResource;

            if (!string.IsNullOrEmpty(label))
                this.StartFromLabel = label;

            if (this.DialogueResource != null)
                this.DialogueLine = await DialogueManager.GetNextDialogueLine(this.DialogueResource, this.StartFromLabel, this._temporaryGameStates);

            base.Show();
        }

        private async void Next(string nextId)
        {
            if (this.DialogueResource != null)
                this.DialogueLine = await DialogueManager.GetNextDialogueLine(this.DialogueResource, nextId, this._temporaryGameStates);
        }

        private async void ApplyDialogueLine()
        {
            if (this.DialogueLine == null)
                return;

            this._mutationCooldown.Stop();

            this._isWaitingForInput = false;
            this._balloon?.FocusMode = Control.FocusModeEnum.All;
            this._balloon?.GrabFocus();

            this._characterLabel?.Visible = !string.IsNullOrEmpty(this.DialogueLine.Character);
            this._characterLabel?.Text = base.Tr(this.DialogueLine.Character, "dialogue");

            var context = this.DialogueLine.ExtraGameStates[2].As<HumanDialogueContext>();

            this._portraitLeft?.Visible = false;
            this._portraitLeft?.Texture = null;
            this._portraitRight?.Visible = false;
            this._portraitRight?.Texture = null;

            if (context?.Initiator?.Name == this.DialogueLine.Character)
            {
                if (context?.Initiator?.HumanPortrait?.Value != null)
                {
                    this._portraitLeft?.Visible = true;
                    this._portraitLeft?.Texture = context.Initiator.HumanPortrait.Value;
                }
            }
            else
            {
                var target = context?.Targets.SingleOrDefault(x => x.Name == this.DialogueLine.Character);
                if (target?.HumanPortrait?.Value != null)
                {
                    this._portraitRight?.Visible = true;
                    this._portraitRight?.Texture = target.HumanPortrait.Value;
                }
            }

            this._dialogueLabel?.Hide();
            this._dialogueLabel?.DialogueLine = this.DialogueLine;

            this._responsesMenu?.Hide();
            this._responsesMenu?.Responses = this.DialogueLine.Responses;

            this._balloon?.Show();
            this._willHideBalloon = false;
            this._dialogueLabel?.Show();

            if (!string.IsNullOrEmpty(this.DialogueLine.Text) && this._dialogueLabel != null)
            {
                this._dialogueLabel.TypeOut();
                await base.ToSignal(this._dialogueLabel, DialogueLabel.SignalName.FinishedTyping);
            }

            if (this.DialogueLine.Responses.Count > 0)
            {
                this._balloon?.FocusMode = Control.FocusModeEnum.None;
                this._responsesMenu?.Show();
            }
            else if (!string.IsNullOrEmpty(this.DialogueLine.Time))
            {
                if (!float.TryParse(this.DialogueLine.Time, out float time))
                    time = this.DialogueLine.Text.Length * 0.02f;

                await base.ToSignal(base.GetTree().CreateTimer(time), Godot.Timer.SignalName.Timeout);
                this.Next(this.DialogueLine.NextId);
            }
            else
            {
                this._isWaitingForInput = true;
                this._balloon?.FocusMode = Control.FocusModeEnum.All;
                this._balloon?.GrabFocus();
            }
        }

        private void OnMutated(Dictionary mutation)
        {
            if (!(bool)mutation["is_inline"])
            {
                this._isWaitingForInput = false;
                this._willHideBalloon = true;
                this._mutationCooldown.Start(0.1f);
            }
        }

        public override void _UnhandledInput(InputEvent @event) =>
            base.GetViewport().SetInputAsHandled();

        public override async void _Notification(int what)
        {
            if (this.DialogueResource != null && this.DialogueLine != null && what == NotificationTranslationChanged && GodotObject.IsInstanceValid(this._dialogueLabel))
            {
                var visibleRatio = this._dialogueLabel.VisibleRatio;
                this.DialogueLine = await DialogueManager.GetNextDialogueLine(this.DialogueResource, this.DialogueLine.Id, this._temporaryGameStates);

                if (visibleRatio < 1.0f)
                    this._dialogueLabel.SkipTyping();
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (GodotObject.IsInstanceValid(this.DialogueLine) && this._dialogueLabel != null)
                this._progress?.Visible = !this._dialogueLabel.IsTyping && this.DialogueLine.Responses.Count == 0 && !this.DialogueLine.HasTag("voice");
        }

        public override void _ExitTree() =>
            DialogueManager.Mutated -= OnMutated;
    }
}
