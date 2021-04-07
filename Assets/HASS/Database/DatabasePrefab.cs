using System;

namespace HASS.Database
{
    /// <summary>
    /// Used internally for the PrefabCollection
    /// </summary>
    [System.Serializable]
    public class DatabasePrefab
    {
        /// <summary>
        /// The path to the asset
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// The Asset type
        /// </summary>
        public Type AssetType { get; set; } = typeof(UnityEngine.Object);

        /// <summary>
        /// The value of AssetDatabase.AssetPathToGUID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Does the prefabObject have a gameObject selected
        /// </summary>
        /// <returns></returns>
        public bool HasAsset()
        {
            return !String.IsNullOrEmpty(AssetPath);
        }
    }
}
