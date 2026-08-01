using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

// <==================== NEAR BY CHARACTERS CONTROLLER ====================> //
partial class Character
{
    /// <summary>
    /// The set of nearby characters within the talking area that have 
    /// an unobstructed line of sight to this character.
    /// </summary>
    public IReadOnlySet<Character> NearByCharacters => this._nearByCharactersController.Characters;

    /// <summary>
    /// The radius of the characters considered as near by.
    /// It is expressed in meters.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m")]
    public float NearByCharactersRadius
    {
        get => this._nearByCharactersController.Radius;
        private set => this._nearByCharactersController.Radius = value;
    }

    private readonly NearByCharactersController _nearByCharactersController = new();

    private sealed partial class NearByCharactersController : Area3D
    {
        public IReadOnlySet<Character> Characters => this._characters;

        public float Radius
        {
            get => ((SphereShape3D)this._collisionShape.Shape).Radius;
            set => ((SphereShape3D)this._collisionShape.Shape).Radius = value;
        }

        private readonly CollisionShape3D _collisionShape = new()
        {
            Name = "CollisionShape",
            Shape = new SphereShape3D() { Radius = 1 }
        };
        private readonly Dictionary<Character, Action> _unsubscribes = [];
        private readonly HashSet<Character> _characters = [];

        public override void _EnterTree()
        {
            base._EnterTree();

            base.BodyEntered += this.OnBodyEntered;
            base.BodyExited += this.OnBodyExited;

            foreach (var (character, handler) in this._unsubscribes)
                character.TreeExiting -= handler;

            this._unsubscribes.Clear();
            this._characters.Clear();

            base.AddChild(this._collisionShape);
        }

        private void OnBodyEntered(Node3D node)
        {
            if (node is Character character && !this._characters.Contains(character))
            {
                if (character == base.GetParent<Character>())
                    return;

                void handler() => this.OnBodyExited(character);

                this._unsubscribes.Add(character, handler);
                character.TreeExiting += handler;
                this._characters.Add(character);
            }
        }

        private void OnBodyExited(Node3D node)
        {
            if (node is Character character && this._characters.Contains(character))
            {
                var handler = this._unsubscribes[character];

                this._characters.Remove(character);
                character.TreeExiting -= handler;
                this._unsubscribes.Remove(character);
            }
        }

        public override void _ExitTree()
        {
            base.RemoveChild(this._collisionShape);

            foreach (var (character, handler) in this._unsubscribes)
                character.TreeExiting -= handler;

            this._unsubscribes.Clear();
            this._characters.Clear();

            base.BodyEntered -= this.OnBodyEntered;
            base.BodyExited -= this.OnBodyExited;

            base._ExitTree();
        }
    }
}
