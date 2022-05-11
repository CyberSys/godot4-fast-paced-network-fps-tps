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
using LiteNetLib.Utils;
using System.Collections.Generic;
using System;

namespace Framework.Network
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NetworkVar : Attribute
    {
        public NetworkSyncFrom From { get; set; } = NetworkSyncFrom.FromServer;
        public NetworkSyncTo To { get; set; } = NetworkSyncTo.ToClient | NetworkSyncTo.ToPuppet;

        public NetworkVar()
        {
        }

        public NetworkVar(NetworkSyncFrom from)
        {
            this.From = from;
        }

        public NetworkVar(NetworkSyncTo to)
        {
            this.To = to;
        }

        public NetworkVar(NetworkSyncFrom from, NetworkSyncTo to)
        {
            this.To = to;
            this.From = from;
        }
    }

    [Flags]
    public enum NetworkSyncFrom
    {
        None = 0,
        FromServer = 1,
        FromClient = 2
    }


    [Flags]
    public enum NetworkSyncTo
    {
        None = 0,
        ToPuppet = 1,
        ToClient = 2,
        ToServer = 3
    }
}