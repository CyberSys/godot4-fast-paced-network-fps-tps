
using System.Linq;
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
        /// <summary>
        /// Change state of collection key
        /// </summary>
        public enum KeyChangeEnum
        {
            /// <summary>
            /// Key was updates 
            /// </summary>
            Update,
            /// <summary>
            /// Key was insert 
            /// </summary>
            Insert,
            /// <summary>
            /// Key was deleted 
            /// /// </summary>
            Delete
        }

        /// <summary>
        /// List of all variables
        /// </summary>
        /// <returns></returns>
        public Vars Vars { get; private set; } = new Vars();

        /// <summary>
        /// Constructor for  vars
        /// </summary>
        /// <param name="vars">Dictonary which contains  varaibles</param>
        public VarsCollection(Vars vars)
        {
            this.Vars = vars;
        }

        /// <summary>
        /// Constructor for  vars
        /// </summary>
        public VarsCollection()
        {
            this.Vars = new Vars();
        }

        /// <summary>
        /// Called when an value inside the collection changed
        /// </summary>
        [Signal]
        public event ValueChangeHandler OnChange;

        /// <summary>
        /// The handler for event changes
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public delegate void ValueChangeHandler(KeyChangeEnum type, string name, string value);

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

                if (default(T) is float)
                {
                    value = value.Replace(",", ".");
                }

                return (T)Convert.ChangeType(value, typeof(T), formatProvider);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get value by given key
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetValue(string varName, string defaultValue = null)
        {
            if (!Vars.AllVariables.ContainsKey(varName))
            {
                return defaultValue;
            }

            return Vars.AllVariables[varName].ToString();
        }


        /// <summary>
        /// Get an key or button id from config
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public object GetKeyValue(string varName, object defaultValue = null)
        {
            if (Vars.AllVariables != null && Vars.AllVariables.ContainsKey(varName))
            {
                var value = Vars.AllVariables[varName].ToString();
                if (value.StartsWith("KEY_"))
                {
                    var key = value.Replace("KEY_", "");
                    Godot.Key myKey;
                    if (Enum.TryParse<Godot.Key>(key, true, out myKey))
                    {
                        return myKey;
                    }
                }
                else if (value.StartsWith("BTN_"))
                {
                    var key = value.Replace("BTN_", "");
                    Godot.MouseButton myBtn;
                    if (Enum.TryParse<Godot.MouseButton>(key, true, out myBtn))
                    {
                        return myBtn;
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Check if key or mouse button pressed
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool IsKeyValuePressed(string varName, object defaultValue)
        {
            var result = GetKeyValue(varName, defaultValue);
            if (result is Godot.Key)
            {
                return Godot.Input.IsKeyPressed((Godot.Key)result);
            }
            else if (result is Godot.MouseButton)
            {
                return Godot.Input.IsMouseButtonPressed((Godot.MouseButton)result);
            }

            return false;
        }


        /// <summary>
        /// Store an value and trigger event
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            lock (Vars.AllVariables)
            {
                if (this.Vars.AllVariables.ContainsKey(key))
                {
                    if (this.Vars.AllVariables[key] != value)
                    {
                        this.Vars.AllVariables[key] = value;
                        this.OnChange?.Invoke(KeyChangeEnum.Update, key, value);
                    }
                }
                else
                {
                    this.Vars.AllVariables.Add(key, value);
                    this.OnChange?.Invoke(KeyChangeEnum.Insert, key, value);
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
            var loadError = cfg.Load("user://" + configFilename);
            if (loadError == Error.Ok)
            {
                Logger.LogDebug(this, "Load config file " + configFilename);
                if (cfg.HasSection("vars"))
                {
                    foreach (var element in Vars.AllVariables.ToArray())
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
            foreach (var element in Vars.AllVariables)
            {
                cfg.SetValue("vars", element.Key, element.Value);
            }

            var loadError = cfg.Save("user://" + configFilename);
            if (loadError != Error.Ok)
            {
                GD.PrintErr("Cant store file " + configFilename + " with reason " + loadError);
            }
            else
            {
                Logger.LogDebug(this, "Store config file " + configFilename);

            }
        }
    }
}