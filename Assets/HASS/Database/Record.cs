using System;
using HASS.Database.Collections;
using HASS.Database.Templates;
using UnityEngine;
namespace HASS.Database
{
    /// <summary>
    /// Abstract Database record class
    /// </summary>
    [System.Serializable]
    public abstract class ARecord : IRecord
    {
        [SerializeField]
        protected int id = -1;
        [SerializeField]
        protected string filterType;
        [SerializeField]
        protected string collectionType;
        [SerializeField]
        protected Type m_Collection = null;
        [SerializeField]
        protected Type m_Filter = null;

        /// <summary>
        /// Sets the collection type and string repersentation becuse unity doesn't Serialize type fields
        /// </summary>
        /// <param name="value">Collection type</param>
        protected void SetCollection(Type value)
        {
            m_Collection = value;
            if (m_Collection != null)
                collectionType = value.ToString();
        }

        /// <summary>
        /// Sets the filter type and string repersentation becuse unity doesn't Serialize type fields
        /// </summary>
        /// <param name="value">Filter type</param>
        protected void SetFilter(Type value)
        {
            m_Filter = value;
            if (m_Filter != null)
                filterType = value.ToString();
        }

        /// <summary>
        /// The Database Collection type
        /// </summary>
        public virtual Type Collection
        {
            get
            {
                if (m_Collection != null) return m_Collection;
                if (!String.IsNullOrEmpty(collectionType))
                {
                    m_Collection = Type.GetType(collectionType);
                    return m_Collection;
                }

                return null;
            }
            set => SetCollection(value);
        }

        /// <summary>
        /// The Database Collection Template type (optional)
        /// </summary>
        public virtual Type Filter
        {
            get
            {
                if (m_Filter != null) return m_Filter;
                if (!String.IsNullOrEmpty(filterType))
                {
                    m_Filter = Type.GetType(filterType);
                    return m_Filter;
                }

                return null;
            }
            set => SetFilter(value);
        }

        /// <summary>
        /// The selected Database Record ID 
        /// </summary>
        public int ID
        {
            get => id;
            set => id = value;
        }
    }

    /// <summary>
    /// Class to reference a database record comes with a custom Drawer.
    /// Record<BlockTemplate> block;
    /// Record<SpawnerTemplate, SpecialChildSpawnerType> specialChildSpawner;
    /// Can be used on a Template or Monobehavior and it will render a dropdown list of records
    /// in the editor
    /// </summary>
    [System.Serializable]
    public class Record : ARecord
    {
    }

    [System.Serializable]
    public class Record<T> : ARecord, IRecord<T> where T : IDatabaseCollection
    {
        [FullSerializer.fsIgnore]
        public override Type Collection
        {
            get => typeof(T);
            set => SetCollection(value);
        }
    }

    [System.Serializable]
    public class Record<T, T1> : ARecord, IRecord<T, T1> where T : IDatabaseCollection where T1 : Template
    {
        [FullSerializer.fsIgnore]
        public override Type Collection
        {
            get => typeof(T);
            set => SetCollection(value);
        }

        [FullSerializer.fsIgnore]
        public override Type Filter
        {
            get => typeof(T1);
            set => SetFilter(value);
        }
    }

    /// <summary>
    /// Database Record Interface
    /// </summary>
    public interface IRecord
    {
        public Type Collection { get; set; }
        public Type Filter { get; set; }
        public int ID { get; set; }
    }

    public interface IRecord<out T, out T1> : IRecord where T : IDatabaseCollection where T1 : Template
    {
    }

    public interface IRecord<out T> : IRecord where T : IDatabaseCollection
    {
    }
}
