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

using RCONServerLib;
using RCONServerLib.Utils;
using System.Net;
using System.Text;
using Godot;
namespace Framework.Network.Services
{
    /// <summary>
    /// The rcon network service
    /// </summary>
    public class RconServerService : IService
    {
        [Signal]
        public event ServerStartedHandler ServerStarted;

        public delegate void ServerStartedHandler(CommandManager manager);

        public const int RconPort = 27020;

        /// <inheritdoc />
        public void Update(float delta)
        {

        }

        /// <inheritdoc />
        public void Render(float delta)
        {

        }

        private RemoteConServer server;

        public CommandManager Commands => server.CommandManager;

        /// <inheritdoc />
        public void Register()
        {
            var server = new RemoteConServer(IPAddress.Any, RconPort)
            {
                SendAuthImmediately = true,
                Debug = true
            };

            server.CommandManager.Add("help", "(command)", "Shows this help", (cmd, arguments) =>
            {
                if (arguments.Count == 1)
                {
                    var helpCmdStr = arguments[0];
                    var helpCmd = server.CommandManager.GetCommand(helpCmdStr);
                    if (helpCmd == null)
                        return "Command not found.";

                    return string.Format("{0} - {1}", helpCmd.Name, helpCmd.Description);
                }

                var sb = new StringBuilder();

                var all = server.CommandManager.Commands.Count;
                var i = 0;
                foreach (var command in server.CommandManager.Commands)
                {
                    if (command.Value.Usage == "")
                        sb.AppendFormat("{0}", command.Value.Name);
                    else
                        sb.AppendFormat("{0} {1}", command.Value.Name, command.Value.Usage);
                    if (i < all)
                        sb.Append(", ");

                    i++;
                }

                return sb.ToString();
            });

            server.StartListening();
            ServerStarted?.Invoke(server.CommandManager);
        }

        /// <inheritdoc />
        public virtual void Unregister()
        {
            this.server?.StopListening();
        }
    }
}
