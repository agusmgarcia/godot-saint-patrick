namespace SaintPatrick;

public sealed partial class Helena : Human
{
    protected override string? GetNextDialogueId(Human? listener)
    {
        return "start";
    }
}