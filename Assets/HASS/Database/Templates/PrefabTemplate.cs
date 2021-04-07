using System;
using System.Collections.Generic;
using System.Linq;

namespace HASS.Database.Templates
{
    /// <summary>
    /// Template for creating prefab records
    /// </summary>
    [System.Serializable]
    public class PrefabTemplate : Template
    {
        public DatabasePrefab prefab;

        public override string Name
        {
            get
            {
                if (prefab != null && prefab.HasAsset())
                {
                    return prefab.AssetPath.Split('/').Last();
                }

                return DefaultName;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets The Asset Path or NULL
        /// </summary>
        /// <returns>The Asset Path or NULL</returns>
        public string GetAssetName()
        {
            if (prefab != null && prefab.HasAsset())
            {
                return prefab.AssetPath;
            }

            return null;
        }
    }
}
