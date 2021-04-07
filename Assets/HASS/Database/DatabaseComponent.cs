using HASS.Database.Collections;
using HASS.Database.Templates;
using UnityEngine;

namespace HASS.Database
{
    /// <summary>
    /// Simple Example Component to Load the Database
    /// </summary>
    public class DatabaseComponent : MonoBehaviour
    {
        public TextAsset dbJson;

        private static DatabaseComponent _instance;
        public static Database DB
        {
            get
            {
                //return TheDatabase.Instance.Database;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DatabaseComponent>();
                    if (_instance == null) Debug.Log($"An instance of {typeof(DatabaseComponent)} is needed in the scene, but there is none.");
                }

                return _instance.Database;
            }
        }

        private Database Database { get; set; }

        private void Awake()
        {
            Database = Database.Load(dbJson);
        }

        public static Object LoadPrefab(int id)
        {
            var collection = (PrefabCollection)DB.Get<PrefabCollection>();
            var prefabItem = (PrefabTemplate)collection.Get(id);
            return collection.LoadPrefab(prefabItem);
        }

        public static Object LoadPrefab(string name)
        {
            var collection = (PrefabCollection)DB.Get<PrefabCollection>();
            var prefabItem = (PrefabTemplate)collection.Get(name);
            return collection.LoadPrefab(prefabItem);
        }
    }
}
