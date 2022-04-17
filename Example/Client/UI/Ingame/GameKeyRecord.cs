using Godot;
using System;

namespace Shooter.Client.UI.Ingame
{
    public partial class GameKeyRecord : HBoxContainer
    {
        public delegate void NotifyNewKey(string keyName);
        public event NotifyNewKey OnKeyChangeStart;

        private string _currentKey;

        public string CollectionKey { get; set; }

        public string currentKey
        {
            get
            {
                return this._currentKey;
            }
            set
            {
                this._currentKey = value;

                if (GetNode("MovementCurrentKey") != null)
                {
                    var labelKey = GetNode("MovementCurrentKey") as Label;
                    labelKey.Text = currentKey.ToString();
                }
            }
        }

        public override void _Ready()
        {
            var label = GetNode("MovementLabel") as Label;
            label.Text = Name;

            var button = GetNode("ChangeKeyButton") as Button;
            button.Pressed += () => { OnKeyChangeStart(Name); };
        }
    }
}
