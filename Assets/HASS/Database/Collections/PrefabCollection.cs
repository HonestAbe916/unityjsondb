using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HASS.Database.Templates;
using UnityEngine;

namespace HASS.Database.Collections
{
    /// <summary>
    /// The Prefab Collection is a special database collection for storing unity prefabs. After saving the database in the Editor,
    /// It will generate a BuildBundle, which can be used to create prefabs without putting them in the resources folder or referencing
    /// them directly on a GameObject. DatabaseComponent.DB.LoadPrefab(prefabName or id);
    ///
    /// To Reference Prefabs in other collections use
    /// public Record<PrefabCollection> prefab;
    /// </summary>
    public class PrefabCollection : DatabaseCollection<PrefabTemplate>
    {
        public const string BundleName = "prefabs.collection.bundle";
        public new String DisplayName { get => "Prefabs"; }
        private AssetBundle m_PrefabBundle = null;

        public override void AfterSave()
        {
            base.AfterSave();

            List<string> assets = new List<string>();
            foreach (var template in List())
            {
                var prefab = (PrefabTemplate)template;
                var newAsset = prefab.GetAssetName();
                if (newAsset != null)
                {
                    assets.Add(newAsset);
                }
            }

#if UNITY_EDITOR
            BuildBundle(BundleName, assets.ToArray());
#endif
        }

        public UnityEngine.Object LoadPrefab(PrefabTemplate template)
        {
            return LoadPrefab(template.Name, template.prefab.AssetType);
        }

        public UnityEngine.Object LoadPrefab(string name, Type t)
        {
            if (m_PrefabBundle == null)
            {
                m_PrefabBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, BundleName));
                if (m_PrefabBundle == null) throw new Exception("Failed to load AssetBundle!");
            }

            var prefab = m_PrefabBundle.LoadAsset(name, t);
            UnityEngine.Object.Instantiate(prefab);

            return prefab;
        }

#if UNITY_EDITOR
        public static void BuildBundle(string name, string[] assetPaths)
        {
            Functions.CreateFolderPath("Assets/StreamingAssets");
            UnityEditor.AssetBundleBuild[] buildMap = new UnityEditor.AssetBundleBuild[1];
            var bundle = new UnityEditor.AssetBundleBuild()
            {
                assetBundleName = name,
                assetNames = assetPaths
            };

            buildMap[0] = bundle;

            UnityEditor.BuildPipeline.BuildAssetBundles(
              Application.streamingAssetsPath,
              buildMap,
              UnityEditor.BuildAssetBundleOptions.None,
              UnityEditor.BuildTarget.StandaloneWindows
            );
        }
#endif
    }
}
