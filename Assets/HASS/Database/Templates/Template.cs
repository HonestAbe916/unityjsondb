using UnityEngine;
using System;
using HASS.JSON;

namespace HASS.Database.Templates
{
    /// <summary>
    /// The Base ScriptableObject that all database items extend from
    /// </summary>
    [System.Serializable]
    public abstract class Template : ScriptableObject
    {
        /// <summary>
        /// Used in the Editor to determine the template we are viewing
        /// </summary>
        public bool Show { get; set; } = true;

        /// <summary>
        /// The Database Record ID
        /// </summary>
        public int ID { get; set; } = -1;

        /// <summary>
        /// The Default Name to use when a new Template is added
        /// </summary>
        public string DefaultName
        {
            get
            {
                return $"{Functions.ExtractNameFromType(this.GetType())} {ID}";
            }
        }

        /// <summary>
        /// The Database Record Name
        /// </summary>
        [FullSerializer.fsProperty]
        public virtual string Name { get; set; }

        /// <summary>
        /// Clone using JSON, can just use CreateInstance<>()?
        /// </summary>
        /// <returns>The cloned Template</returns>
        public Template Clone()
        {
            string json = JsonSerialization.Serialize(this.GetType(), this);
            return (Template)JsonSerialization.Deserialize(this.GetType(), json);
        }

        /// <summary>
        /// Called before the record is saved to json in the editor
        /// </summary>
        public virtual void BeforeSave()
        {
        }

        /// <summary>
        /// Called in the Editor OnGui Update
        /// </summary>
        public virtual void OnUpdate()
        {
        }
    }
}