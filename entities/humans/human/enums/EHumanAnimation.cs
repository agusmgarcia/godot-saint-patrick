namespace SaintPatrick.Entities.Humans.Human.Enums;


/// <summary>
/// Identifies the logical animation group played by HumanAnimationPlayer.
/// </summary>
public enum EHumanAnimation
{
    /// <summary>Idle animation played while drunk.</summary>
    DrunkIdle,
    /// <summary>Run animation played while drunk.</summary>
    DrunkRun,
    /// <summary>Walk animation played while drunk.</summary>
    DrunkWalk,
    /// <summary>Airborne fall animation.</summary>
    Fall,
    /// <summary>Brief animation of the human swatting away a fly.</summary>
    FlyRemoval,
    /// <summary>Standard idle stand animation.</summary>
    Idle,
    /// <summary>Landing animation played on touching the floor after a fall.</summary>
    Land,
    /// <summary>Standard run animation.</summary>
    Run,
    /// <summary>Conversation/talk animation.</summary>
    Talk,
    /// <summary>Standard walk animation.</summary>
    Walk
}