using Godot;

namespace SaintPatrick;

public partial class Helena : Character
{
    public Helena()
        : base(new() { Gender = EGender.Female })
    {
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("toogle_drunk"))
            this.Drunk = !this.Drunk;
    }
}
