using System.Threading;
using Godot;
using System;

namespace Shooter.Client.UI.Ingame
{

    public partial class KeyConfirmationDialog : ConfirmationDialog
    {
        public string selectedKey;
        public string keyName;

        private bool isSelected = false;
        public void openChanger(string keyName)
        {
            this.keyName = keyName;
            this.DialogText = "Press a key to continue...";
            this.PopupCentered();
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (!isSelected)
            {

                // Receives key input
                if (@event is InputEventKey)
                {
                    var ev = @event as InputEventKey;
                    selectedKey = "KEY_" + ev.Keycode;
                    this.DialogText = "Current selected key is " + ev.Keycode.ToString() + ". Please press apply to confirm.";
                    isSelected = true;
                }

                if (@event is InputEventMouseButton)
                {
                    var ev = @event as InputEventMouseButton;
                    selectedKey = "BTN_" + ev.ButtonIndex;
                    this.DialogText = "Current selected button is " + ev.ButtonIndex.ToString() + ". Please press apply to confirm.";
                    isSelected = true;
                }
            }

            @event.Dispose();
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            GetOkButton().FocusMode = Control.FocusModeEnum.None;
            GetCancelButton().FocusMode = Control.FocusModeEnum.None;
        }
    }
}