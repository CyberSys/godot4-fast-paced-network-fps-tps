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
using Framework.Network.Commands;
using System.Linq;
using Framework.Game;
using LiteNetLib.Utils;
using Framework.Game.Server;

namespace Framework.Network
{
    /// <summary>
    /// Player class for network players eg. players, npcs
    /// </summary>
    public abstract partial class NetworkPlayer : Player
    {
        /// <summary>
        /// Get the current network state
        /// </summary>
        /// <returns></returns>
        public virtual PlayerState ToNetworkState()
        {
            var netComps = new System.Collections.Generic.Dictionary<int, byte[]>();
            foreach (var component in this.Components.All.Where(df => df.GetType().GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IChildNetworkSyncComponent<>))))
            {
                var findIndex = this.AvaiablePlayerComponents.FindIndex(df => df.NodeType == component.GetType());
                if (findIndex > -1)
                {
                    var instanceMethod = component.GetType().GetMethod("GetNetworkState");
                    INetSerializable result = instanceMethod.Invoke(component, new object[] { }) as INetSerializable;

                    var writer = new NetDataWriter();
                    result.Serialize(writer);
                    netComps.Add(findIndex, writer.Data);
                }
            }

            return new PlayerState
            {
                Id = this.Id,
                NetworkComponents = netComps
            };
        }

        /// <summary>
        /// Apply an network state
        /// </summary>
        /// <param name="state">The network state to applied</param>
        public virtual void ApplyNetworkState(PlayerState state)
        {
            foreach (var component in this.Components.All.Where(df => df.GetType().GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IChildNetworkSyncComponent<>))))
            {
                var findIndex = this.AvaiablePlayerComponents.FindIndex(df => df.NodeType == component.GetType());
                if (findIndex > -1)
                {
                    if (state.NetworkComponents != null && state.NetworkComponents.ContainsKey(findIndex))
                    {
                        //     var decompose = state.Decompose();
                        var instanceMethod = component.GetType().GetMethod("ApplyNetworkState");
                        var parameterType = instanceMethod.GetParameters().First().ParameterType;

                        var methods = state.GetType().GetMethods();
                        var method = methods.Single(mi => mi.Name == "Decompose" && mi.GetParameters().Count() == 1);

                        var decomposed = method.MakeGenericMethod(parameterType)
                              .Invoke(state, new object[] { findIndex });
                        instanceMethod.Invoke(component, new object[] { decomposed });
                    }
                }
            }
        }
    }
}
