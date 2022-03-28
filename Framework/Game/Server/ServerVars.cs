using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;
using System.Collections.Generic;
using System;
using System.Globalization;
using LiteNetLib.Utils;
using Framework.Game.Server;

namespace Framework.Game.Server
{
    /// <summary>
    /// Core class for server variables
    /// </summary>
    public struct ServerVars : INetSerializable
    {
        /// <summary>
        /// Dictonary which contains server varaibles
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> AllVariables { get; set; }

        /// <summary>
        /// Constructor for server vars
        /// </summary>
        /// <param name="vars">Dictonary which contains server varaibles</param>
        public ServerVars(Dictionary<string, string> vars)
        {
            this.AllVariables = vars;
        }

        /// <summary>
        /// Get an server variable
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string varName, T defaultValue = default(T)) where T : IConvertible
        {
            if (!AllVariables.ContainsKey(varName))
            {
                return defaultValue;
            }
            try
            {
                var value = AllVariables[varName].ToString();
                var formatProvider = CultureInfo.InvariantCulture;
                return (T)Convert.ChangeType(AllVariables[varName].ToString(), typeof(T), formatProvider);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.AllVariables);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.AllVariables = reader.GetDictonaryString();
        }
    }
}