using System.Linq;
using System;
using System.Collections.Generic;
using Godot;
using Framework.Input;
using Framework.Network;
using Framework;
using Framework.Physics;

namespace Framework.Input
{

    /// <summary>
    /// The base input processor abstract class
    /// </summary>
    public class BaseInputProcessor : IInputProcessor
    {
        /// <inheritdoc />  
        public bool InputEnabled { get; set; } = true;

        /// <inheritdoc />
        public List<string> AvaiableInputs => this.GetKeys().Keys.ToList();

        /// <inheritdoc />  
        public Quaternion ViewRotation { get; set; }

        /// <summary>
        /// Get an list of keys 
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, bool> GetKeys()
        {
            return new Dictionary<string, bool>();
        }

        /// <summary>
        /// Get the player input
        /// </summary>
        /// <returns></returns>
        public virtual GeneralPlayerInput GetPlayerInput()
        {
            return new GeneralPlayerInput();
        }
    }

    /// <summary>
    /// An default input procsessor
    /// </summary>
    public class GeneralInputProcessor : BaseInputProcessor
    {
        /// <inheritdoc />  
        public override Dictionary<String, bool> GetKeys()
        {
            return new Dictionary<string, bool>{
                    { "Forward", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_forward", Godot.Key.W)},
                    { "Back", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_backward", Godot.Key.S)},
                    { "Right", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_right", Godot.Key.D)},
                    { "Left", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_left", Godot.Key.A)},
                    { "Jump", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_jump", Godot.Key.Space)},
                    { "Crouch", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_crouch", Godot.Key.Ctrl)},
                    { "Shifting", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_shift", Godot.Key.Shift)},
                    { "Fire",  Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_attack", Godot.MouseButton.Left)},
                };
        }

        /// <inheritdoc />  
        public override GeneralPlayerInput GetPlayerInput()
        {
            var input = new GeneralPlayerInput();
            var activeKeys = new Dictionary<string, bool>();
            if (Godot.Input.GetMouseMode() == Godot.Input.MouseMode.Captured && InputEnabled)
            {
                activeKeys = this.GetKeys();
            }

            input.ViewDirection = ViewRotation == new Quaternion(0, 0, 0, 0) ? Quaternion.Identity : ViewRotation;
            input.Apply(this.AvaiableInputs, activeKeys);

            return input;
        }
    }
}
