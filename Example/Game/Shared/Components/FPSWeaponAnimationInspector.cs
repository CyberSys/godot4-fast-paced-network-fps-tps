using System;
using Godot;
using Framework.Network;
using Framework;
using LiteNetLib.Utils;
using Framework.Physics;
using Framework.Utils;
using Framework.Game.Client;
using Framework.Game;
using Framework.Input;
namespace Shooter.Shared.Components
{
    public partial class FPSWeaponAnimator : Node, IPlayerComponent
    {
        public override Godot.Collections.Array _GetPropertyList()
        {
            var arr = new Godot.Collections.Array();
            //section
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "WeaponAnimation"},
                {"type",  Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Category  | PropertyUsageFlags.Editor}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "ShootTest"},
                {"type", Variant.Type.Bool },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            //kick back            
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "Kick Back (On shoot)"},
                {"type",  Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Group  | PropertyUsageFlags.Editor},
                {"hint_string", "RecoilKickBack"}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "RecoilKickBackRotation"},
                {"type", Variant.Type.Vector3 },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "RecoilKickBackPositionSpeed"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "RecoilKickBackPosition"},
                {"type", Variant.Type.Vector3 },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "RecoilKickBackRotationSpeed"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "RecoilKickBackEnabled"},
                {"type", Variant.Type.Bool },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            //camera            
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "Camera Recoil (On shoot)"},
                {"type",  Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Group  | PropertyUsageFlags.Editor},
                {"hint_string", "CameraRecoil"}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "CameraRecoilPosition"},
                {"type", Variant.Type.Vector3 },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "CameraRecoilRotationSpeed"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "CameraRecoilRotationReturnSpeed"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            //Sway move            
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "Sway Move (On mouse move)"},
                {"type",  Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Group  | PropertyUsageFlags.Editor},
                {"hint_string", "SwayLookMove"}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayLookMoveEnabled"},
                {"type", Variant.Type.Bool },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayLookMoveAmount"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayLookMoveSmoothing"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayLookMoveResetSmoothing"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayLookMoveMax"},
                {"type", Variant.Type.Vector2 },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            // Idle sway
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "Idle Sway (Breathing)"},
                {"type",  Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Group  | PropertyUsageFlags.Editor},
                {"hint_string", "IdleSway"}
            });


            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "IdleSwayEnabled"},
                {"type", Variant.Type.Bool },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "IdleSwayAmountA"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "IdleSwayAmountB"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "IdleSwayScale"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "IdleSwayLerpSpeed"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            // Walk sway bob
            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "Sway bobbing (On walk)"},
                {"type",  Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Group  | PropertyUsageFlags.Editor},
                {"hint_string", "SwayWalkBob"}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayWalkBobEnabled"},
                {"type", Variant.Type.Bool },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayWalkBobSpeed"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            arr.Add(new Godot.Collections.Dictionary()
            {
                {"name", "SwayWalkBobDistance"},
                {"type", Variant.Type.Float },
                { "usage",  PropertyUsageFlags.Editor | PropertyUsageFlags.Storage}
            });

            return arr;
        }
    }
}
