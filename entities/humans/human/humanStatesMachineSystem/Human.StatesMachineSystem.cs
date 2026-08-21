using System;
using Godot;
using SaintPatrick.Systems;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO:
/// </summary>
[GlobalClass]
public sealed partial class HumanStatesMachineSystem : StatesMachineSystem<Human, HumanBaseState>
{
    public HumanStatesMachineSystem()
        : base(typeof(HumanIdleState), (ValueTuple)(ValueType)new HumanIdleStateInitParams() { })
    {
    }
}