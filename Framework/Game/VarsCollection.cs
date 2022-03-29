
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
using System;
using System.Globalization;

namespace Framework.Game
{
    /// <summary>
    /// An collection of vars and config files
    /// </summary>
    public class VarsCollection
    {
        internal Vars _vars = new Vars();
        public Vars Vars => _vars;

        /// <summary>
        /// Constructor for  vars
        /// </summary>
        /// <param name="vars">Dictonary which contains  varaibles</param>
        public VarsCollection(Vars vars)
        {
            this._vars = vars;
        }

        /// <summary>
        /// Constructor for  vars
        /// </summary>
        public VarsCollection()
        {
            this._vars = new Vars();
        }

        /// <summary>
        /// Called when an value inside the collection changed
        /// </summary>
        [Signal]
        public event ValueChangeHandler OnChange;
        /// <summary>
        /// The handler for event changes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public delegate void ValueChangeHandler(string name, string value);

        /// <summary>
        /// Get an server variable
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string varName, T defaultValue = default(T)) where T : IConvertible
        {
            if (!Vars.AllVariables.ContainsKey(varName))
            {
                return defaultValue;
            }
            try
            {
                var value = Vars.AllVariables[varName].ToString();
                var formatProvider = CultureInfo.InvariantCulture;
                return (T)Convert.ChangeType(Vars.AllVariables[varName].ToString(), typeof(T), formatProvider);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Store an value and trigger event
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            if (this.Vars.AllVariables.ContainsKey(key))
            {
                if (this.Vars.AllVariables[key] != value)
                {
                    this.Vars.AllVariables[key] = value;
                    this.OnChange?.Invoke(key, value);
                }
            }
        }

        /// <summary>
        /// Load an config file by given filename
        /// </summary>
        /// <param name="configFilename"></param>
        public void LoadConfig(string configFilename)
        {
            var cfg = new Godot.ConfigFile();
            if (cfg.Load("user://" + configFilename) != Error.Ok)
            {
                if (cfg.HasSection("vars"))
                {
                    foreach (var element in Vars.AllVariables)
                    {
                        if (cfg.GetValue("vars", element.Key) != null)
                        {
                            this.Set(element.Key, cfg.GetValue("vars", element.Key).ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Store an config file by given values
        /// </summary>
        /// <param name="configFilename"></param>
        public void StoreConfig(string configFilename)
        {
            var cfg = new Godot.ConfigFile();
            if (cfg.Load("user://" + configFilename) != Error.Ok)
            {
                foreach (var element in Vars.AllVariables)
                {
                    cfg.SetValue("vars", element.Key, element.Value);
                }
            }

            cfg.Save("user://" + configFilename);
        }
    }
}