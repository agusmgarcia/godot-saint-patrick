using System.Collections.Generic;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// An <see cref="AnimationPlayer"/> component that automatically corrects the owning
/// entity's <c>Model</c> node Y-position whenever a new animation starts, compensating
/// for root-motion offsets baked into Mixamo animation clips.
/// <para>
/// Offsets are computed <em>automatically</em> once on <see cref="_EnterTree"/> by scanning
/// every loaded <see cref="AnimationLibrary"/>: for each animation the root bone's Y position
/// at time 0 is sampled. The animation with the highest Hips Y (the most upright pose,
/// naturally the standing idle) becomes the zero-correction baseline, and every other
/// animation receives a positive offset equal to the difference. The results are cached in
/// an internal dictionary so each per-animation look-up during gameplay is O(1).
/// </para>
/// <para>
/// The cached offsets are in Mixamo's unscaled coordinate space. When applying the
/// correction, the offset is multiplied by the <c>Model</c> node's Y scale so characters
/// of different heights are all corrected accurately.
/// </para>
/// <para>
/// The correction is applied smoothly each frame via linear interpolation rather than
/// snapping instantly, avoiding visible pops when animations transition.
/// </para>
/// </summary>
public partial class CorrectedAnimationPlayer : AnimationPlayer
{
    /// <summary>
    /// Speed (in units per second) at which the model's Y-position is interpolated toward
    /// the target offset. Higher values make the correction snap faster; lower values produce
    /// a smoother, more gradual transition between animation root-motion offsets.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float LerpSpeed { get; private set; } = 5.0f;

    private readonly NodeTracker<Node3D> _modelTracker = new();

    private Vector3 _targetPosition = Vector3.Zero;
    private Node3D? _model;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(base.CurrentAnimation);

        this._modelTracker.NodeTracked += this.OnModelTracked;
        this._modelTracker.NodeUntracked += this.OnModelUntracked;
        this._modelTracker.Track(base.GetOwner());
    }

    private void OnAnimationStarted(StringName animationName)
    {
        if (string.IsNullOrEmpty(animationName))
            return;

        if (this._model == null)
            return;

        this._targetPosition = AnimationOffsetsPool.Get(this, animationName) * this._model.Scale.Y;
    }

    private void OnModelTracked(Node3D maybeModel)
    {
        if (maybeModel.Name != "Model")
            return;

        this._model = maybeModel;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this._model == null)
            return;

        this._model.Position = this._model.Position.Lerp(
            this._targetPosition,
            (float)delta * this.LerpSpeed);
    }

    private void OnModelUntracked(Node3D maybeModel)
    {
        if (this._model != maybeModel)
            return;

        this._model = null;
    }

    public override void _ExitTree()
    {
        this._modelTracker.Untrack();
        this._modelTracker.NodeUntracked -= this.OnModelUntracked;
        this._modelTracker.NodeTracked -= this.OnModelTracked;

        base.AnimationStarted -= this.OnAnimationStarted;

        base._ExitTree();
    }

    private static class AnimationOffsetsPool
    {
        private static readonly Dictionary<StringName, Vector3> _ANIMATION_OFFSETS = [];

        public static Vector3 Get(AnimationPlayer animationPlayer, StringName animationName)
        {
            if (AnimationOffsetsPool._ANIMATION_OFFSETS.TryGetValue(animationName, out var result))
                return result;

            var samples = new Dictionary<StringName, float>();
            var maxHipsY = float.MinValue;

            foreach (var libraryName in animationPlayer.GetAnimationLibraryList())
            {
                var library = animationPlayer.GetAnimationLibrary(libraryName);

                foreach (var animName in library.GetAnimationList())
                {
                    var hipsY = AnimationOffsetsPool.SampleRootBoneYAtStart(library.GetAnimation(animName));
                    if (!hipsY.HasValue)
                        continue;

                    samples[new StringName(string.IsNullOrEmpty(libraryName)
                        ? animName
                        : $"{libraryName}/{animName}")] = hipsY.Value;

                    if (hipsY.Value > maxHipsY)
                        maxHipsY = hipsY.Value;
                }
            }

            foreach (var (fullName, hipsY) in samples)
            {
                float offset = maxHipsY - hipsY;
                if (offset > 0.001f)
                    AnimationOffsetsPool._ANIMATION_OFFSETS[fullName] = new Vector3(0f, offset, 0f);
            }

            return AnimationOffsetsPool._ANIMATION_OFFSETS.GetValueOrDefault(animationName, Vector3.Zero);
        }

        private static float? SampleRootBoneYAtStart(Animation animation)
        {
            for (int i = 0; i < animation.GetTrackCount(); i++)
            {
                if (animation.TrackGetType(i) != Animation.TrackType.Position3D)
                    continue;

                if (animation.TrackGetPath(i).GetConcatenatedSubNames() != "mixamorig_Hips")
                    continue;

                return animation.PositionTrackInterpolate(i, 0.0).Y;
            }

            return null;
        }
    }
}
