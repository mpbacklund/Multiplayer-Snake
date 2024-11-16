using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace Snake.Storage
{
    /// <summary>
    /// This class demonstrates how to create an object that can be serialized
    /// under the XNA framework.
    /// </summary>
    //[Serializable]
    [DataContract(Name = "KeyData")]
    public class KeyData
    {
        /// <summary>
        /// Have to have a default constructor for the XmlSerializer.Deserialize method
        /// </summary>
        public KeyData()
        {
        }

        /// <summary>
        /// Overloaded constructor used to create an object for long term storage
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Direction"></param>
        public KeyData(Shared.Components.Input.Type direction, Keys key)
        {
            Key = key;
            Direction = direction;

            keys.Add(1, "one");
            keys.Add(2, "two");
        }

        [DataMember()]
        public Shared.Components.Input.Type Direction { get; set; }

        [DataMember()]
        public Keys Key { get; set; }

        [DataMember()]
        public Dictionary<int, string> keys = new Dictionary<int, string>();
    }
}
