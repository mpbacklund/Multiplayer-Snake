using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Snake.Storage
{
    /// <summary>
    /// This class demonstrates how to create an object that can be serialized
    /// under the XNA framework.
    /// </summary>
    //[Serializable]
    [DataContract(Name = "ScoreData")]
    public class ScoreData
    {
        /// <summary>
        /// Have to have a default constructor for the XmlSerializer.Deserialize method
        /// </summary>
        public ScoreData()
        {
        }

        /// <summary>
        /// Overloaded constructor used to create an object for long term storage
        /// </summary>
        /// <param name="score"></param>
        /// <param name="name"></param>
        public ScoreData(int score, string name)
        {
            Name = name;
            Score = score;
            TimeStamp = DateTime.Now;

            keys.Add(1, "one");
            keys.Add(2, "two");
        }

        [DataMember()]
        public string Name { get; set; }
        [DataMember()]
        public int Score { get; set; }
        [DataMember()]
        public DateTime TimeStamp { get; set; }
        [DataMember()]
        public Dictionary<int, string> keys = new Dictionary<int, string>();
    }
}
