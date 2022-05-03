/*
 * Created on Mon Mar 28 2022
 *
 * The MIT License (MIT)
 * Copyright (c) 2022 Stefan Boronczyk, Striked GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Godot;
using Framework.Game;
using Framework.Game.Server;
using Framework;
using Framework.Network.Commands;
using LiteNetLib;
using Framework.Network.Services;
using System;
using System.Linq;

using System.Collections.Generic;

namespace Framework.Game.Client
{
    /// <summary>
    /// Is the base class for any client (SubViewport)
    /// </summary>
    public class ClientLogic : GameLogic
    {
        /// <summary>
        /// The dictonary with all default settings (vars);
        /// </summary>
        [Export]
        public Dictionary<string, string> DefaultVars = new Dictionary<string, string>
        {
           { "cl_sensitivity_x", "2.0"},
           { "cl_sensitivity_y", "2.0"},
           { "cl_debug_server", "true"},

           { "cl_fov", "70"},

           { "cl_resolution", "640x480"},
           { "cl_draw_shadow", "SoftLow"},

           { "cl_window_mode", ClientSettings.WindowModes.Windowed.ToString()},
           { "cl_draw_msaa", Godot.Viewport.MSAA.Msaa2x.ToString()},
           { "cl_draw_aa", Godot.Viewport.ScreenSpaceAA.Disabled.ToString()},
           { "cl_draw_debug",  Godot.Viewport.DebugDrawEnum.Disabled.ToString()},

           { "cl_draw_glow", "false"},
           { "cl_draw_sdfgi", "false"},
           { "cl_draw_ssao", "false"},
           { "cl_draw_ssil", "false"},
           { "cl_draw_occulision", "false"},
           { "cl_draw_debanding", "false"},
           { "cl_draw_vsync", "false"},

           // movement
           {"key_forward", "KEY_W"},
           {"key_backward", "KEY_S"},
           {"key_right", "KEY_D"},
           {"key_left", "KEY_A"},
           {"key_jump", "KEY_Space"},
           {"key_crouch", "KEY_Ctrl"},
           {"key_shift", "KEY_Shift"},
           {"key_attack", "BTN_Left"},
        };

        /// <inheritdoc />
        private ClientNetworkService netService = null;

        /// <inheritdoc />
        private string loadedWorldName = null;

        /// <summary>
        /// Create an client world
        /// </summary>
        /// <returns></returns>
        public virtual ClientWorld CreateWorld()
        {
            return new ClientWorld();
        }

        /// <inheritdoc />
        internal override void OnMapInstanceInternal(PackedScene res, uint worldTick)
        {
            ClientWorld newWorld = this.CreateWorld();
            newWorld.Name = "world";

            this.AddChild(newWorld);

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;

            //send server map loading was completed
            this.netService.SendMessageSerialisable<ServerInitializer>(0, new ServerInitializer());
            this.AfterMapLoaded();
        }

        /// <inheritdoc />        
        internal override void DestroyMapInternal()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.AfterMapDestroy();
        }

        /// <summary>
        /// Disconnect the client
        /// </summary>
        public void Disconnect()
        {
            this.netService.Disconnect();
            this.DestroyMapInternal();
        }

        /// <summary>
        /// Connect with an server
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(string hostname, int port)
        {
            if (this.CurrentWorld == null)
            {
                this.netService.Connect(new ClientConnectionSettings
                {
                    Port = port,
                    Hostname = hostname,
                    SecureKey = this.secureConnectionKey
                });
            }
        }

        /// <summary>
        /// On local client is connected
        /// </summary>
        public virtual void OnConnected()
        {

        }

        /// <summary>
        /// On local client are disconneted
        /// </summary>
        public virtual void OnDisconnect()
        {

        }

        /// <inheritdoc />  
        internal override void InternalTreeEntered()
        {
            ClientSettings.Variables = new VarsCollection(new Vars(this.DefaultVars));
            ClientSettings.Variables.LoadConfig("client.cfg");
            ClientSettings.Variables.OnChange += (name, value) =>
            {
                if (name == "cl_resolution")
                {
                    applyResolution(value);
                }

                if (name == "cl_window_mode")
                {
                    applyWindowMode(value);
                }

                if (name == "cl_draw_debug")
                {
                    applyDebug(value);
                }

                if (name == "cl_draw_aa")
                {
                    applyAA(value);
                }

                if (name == "cl_draw_msaa")
                {
                    applyMSAA(value);
                }

                if (name == "cl_draw_shadow")
                {
                    applyShadow(value);
                }

                if (name == "cl_draw_occulision")
                {
                    applyOcclusion(ClientSettings.Variables.Get<bool>("cl_draw_occulision"));
                }

                if (name == "cl_draw_debanding")
                {
                    applyDebanding(ClientSettings.Variables.Get<bool>("cl_draw_debanding"));
                }

                if (name == "cl_draw_vsync")
                {
                    applyVsync(ClientSettings.Variables.Get<bool>("cl_draw_vsync"));
                }
            };

            this.AudioListenerEnable3d = true;

            this.netService = this.Services.Create<ClientNetworkService>();
            this.netService.OnDisconnect += this.OnInternalDisconnect;
            this.netService.Connected += this.OnConnected;

            this.netService.SubscribeSerialisable<ClientWorldLoader>((package, peer) =>
            {
                if (this.loadedWorldName != package.WorldName)
                {
                    this.loadedWorldName = package.WorldName;
                    this.LoadWorldInternal(package.WorldName, package.WorldTick);
                }
            });


            base.InternalTreeEntered();
        }

        internal override void InternalReady()
        {
            applyWindowMode(ClientSettings.Variables.GetValue("cl_window_mode", "Windowed"));
            applyResolution(ClientSettings.Variables.GetValue("cl_resolution", "640x480"));
            applyDebug(ClientSettings.Variables.GetValue("cl_draw_debug", "Disabled"));
            applyAA(ClientSettings.Variables.GetValue("cl_draw_aa", "Disabled"));
            applyMSAA(ClientSettings.Variables.GetValue("cl_draw_msaa", "Msaa2x"));
            applyShadow(ClientSettings.Variables.GetValue("cl_draw_shadow", "SoftLow"));
            applyOcclusion(ClientSettings.Variables.Get<bool>("cl_draw_occulision", false));
            applyDebanding(ClientSettings.Variables.Get<bool>("cl_draw_debanding", false));
            applyVsync(ClientSettings.Variables.Get<bool>("cl_draw_vsync", false));
        }

        internal void applyOcclusion(bool isEnabled)
        {
            this.UseOcclusionCulling = isEnabled;
        }

        internal void applyVsync(bool isEnabled)
        {
            if (isEnabled)
            {
                //  DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled, 0);
            }
            else
            {
                // DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled, 0);
            }
        }

        internal void applyDebanding(bool isEnabled)
        {
            this.UseDebanding = isEnabled;
        }

        internal void applyDebug(string debug)
        {
            Godot.Viewport.DebugDrawEnum result;
            if (Enum.TryParse<Godot.Viewport.DebugDrawEnum>(debug, true, out result))
            {
                this.DebugDraw = result;
            }
        }

        internal void applyAA(string debug)
        {
            Godot.Viewport.ScreenSpaceAA result;
            if (Enum.TryParse<Godot.Viewport.ScreenSpaceAA>(debug, true, out result))
            {
                this.ScreenSpaceAa = result;
            }
        }

        internal void applyMSAA(string debug)
        {
            Godot.Viewport.MSAA result;
            if (Enum.TryParse<Godot.Viewport.MSAA>(debug, true, out result))
            {
                this.Msaa = result;
            }
        }

        internal void applyShadow(string shadowLevel)
        {
            RenderingServer.ShadowQuality result;
            if (Enum.TryParse<RenderingServer.ShadowQuality>(shadowLevel, true, out result))
            {
                RenderingServer.ShadowsQualitySet(result);
            }
        }

        internal void applyResolution(string resolution)
        {
            if (ClientSettings.Resolutions.Contains(resolution))
            {
                var values = resolution.Split("x");
                var res = new Vector2i(int.Parse(values[0]), int.Parse(values[1]));

                DisplayServer.WindowSetSize(res);
                this.GetTree().Root.ContentScaleSize = res;
            }
        }

        internal void applyWindowMode(string windowMode)
        {
            ClientSettings.WindowModes mode;
            if (Enum.TryParse<ClientSettings.WindowModes>(windowMode, true, out mode))
            {
                if (mode == ClientSettings.WindowModes.Borderless)
                {
                    this.GetTree().Root.Mode = Window.ModeEnum.Windowed;
                    this.GetTree().Root.Borderless = true;
                }
                else if (mode == ClientSettings.WindowModes.Windowed)
                {
                    this.GetTree().Root.Mode = Window.ModeEnum.Windowed;
                    this.GetTree().Root.Borderless = false;
                }
                else if (mode == ClientSettings.WindowModes.Fullscreen)
                {
                    this.GetTree().Root.Mode = Window.ModeEnum.Fullscreen;
                    this.GetTree().Root.Borderless = false;
                }
                else if (mode == ClientSettings.WindowModes.ExclusiveFullscreen)
                {
                    this.GetTree().Root.Mode = Window.ModeEnum.ExclusiveFullscreen;
                    this.GetTree().Root.Borderless = false;
                }
            }
        }

        /// <inheritdoc />  
        private void OnInternalDisconnect(DisconnectReason reason, bool fullDisconnect)
        {
            if (fullDisconnect)
            {
                Logger.LogDebug(this, "Full disconnected");
                this.OnDisconnect();
                this.DestroyMapInternal();

            }
        }
    }
}
