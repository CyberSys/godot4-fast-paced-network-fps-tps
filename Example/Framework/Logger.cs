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

using System.Collections.Generic;
using System.Globalization;
using System;
using System.Diagnostics;

namespace Framework
{
    /// <summary>
    /// Custom logger of the framework
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Event triggered when an new log message received
        /// </summary>
        public static event LogMessageHandler OnLogMessage;

        /// <summary>
        /// The log message handler
        /// </summary>
        /// <param name="message"></param>
        public delegate void LogMessageHandler(string message);

        /// <summary>
        /// Currently not in use! (TODO)
        /// </summary>
        /// <typeparam name="string">Key</typeparam>
        /// <typeparam name="string">Value</typeparam>
        /// <returns></returns>
        public static Dictionary<string, string> DebugUI = new Dictionary<string, string>();

        /// <summary>
        /// For client-side logging (not finish)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetDebugUI(string name, string value)
        {
            DebugUI[name] = value;
        }

        /// <summary>
        /// Debug logging an object with messsage
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        public static void LogDebug(object service, string message)
        {
            var format = String.Format(
                    "[{3}][{0}][{1}] {2}",
                    Process.GetCurrentProcess().StartTime,
                    service.GetType().Name,
                    message,
                    System.Threading.Thread.CurrentThread.ManagedThreadId
                );

            System.Diagnostics.Debug.WriteLine(format);
            Logger.OnLogMessage?.Invoke(message);
        }
    }
}
