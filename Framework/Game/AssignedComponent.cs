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

using System;
namespace Framework.Game
{
    /// <summary>
    /// An assignable component for remote component injection
    /// </summary>
    public struct AssignedComponent
    {
        /// <summary>
        /// The type of the child node which be injected to an remote base component (eg. player)
        /// </summary>
        /// <value></value>
        public Type NodeType { get; set; }

        /// <summary>
        /// The resourcepath (if exist) to the resource which will be injected
        /// </summary>
        /// <value></value>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Constrcutor for an assignable component
        /// </summary>
        /// <param name="NodeType">The type of the child node which be injected to an remote base component (eg. player)</param>
        /// <param name="ResourcePath">The resourcepath (if exist) to the resource which will be injected</param>
        public AssignedComponent(Type NodeType, string ResourcePath = null)
        {
            this.NodeType = NodeType;
            this.ResourcePath = ResourcePath;
        }
    }
}