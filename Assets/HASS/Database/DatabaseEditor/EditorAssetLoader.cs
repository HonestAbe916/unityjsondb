# if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HASS.Database.DatabaseEditor
{
    /// <summary>
    /// Loads assets for use in the editor
    /// </summary>
    public static class EditorAssetLoader
    {
        /// <summary>
        /// Cache of the databases references
        /// </summary>
        private static Dictionary<string, Database> databaseCache = new Dictionary<string, Database>();
        
        /// <summary>
        /// Loads a Database for the Editor
        /// </summary>
        /// <param name="production">Load the Production Database?</param>
        /// <returns></returns>
        public static Database GetDB(bool production = false)
        {
            var currentDBName = production ? DatabaseSettings.Settings.databaseName : DatabaseSettings.Settings.ViewingDatabase;
            if (databaseCache.ContainsKey(currentDBName) && !NeedRefresh(databaseCache[currentDBName]))
            {
                return databaseCache[currentDBName];
            }

            var newDb = Database.EditorLoad(production);
            if (newDb != null)
            {
                AddToCache(currentDBName, newDb);
                return databaseCache[currentDBName];
            }
            else
            {
                var fullPath = DatabaseSettings.GetFullPath(currentDBName);
                var fileExists = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
                if (fileExists == null)
                {
                    newDb = new Database();
                    Database.Save(newDb);
                    AddToCache(currentDBName, newDb);
                    return databaseCache[currentDBName];
                }

                return null;
            }
        }

        /// <summary>
        /// Adds a database to the cache
        /// </summary>
        /// <param name="key">Database Name</param>
        /// <param name="value">Database</param>
        private static void AddToCache(string key, Database value)
        {
            if (!databaseCache.ContainsKey(key))
            {
                databaseCache.Add(key, value);
            }
            else
            {
                databaseCache[key] = value;
            }
        }


        /// <summary>
        /// After clicking play and stoppping in the editor, the database Templates become null
        /// not sure why yet...
        /// </summary>
        /// <returns></returns>
        private static bool NeedRefresh(Database db)
        {
            if (db == null) return true;

            if (db.First() == null) return true;

            var collection = db.First();

            if (collection.List().Count > 0 && collection.List()[0] == null)
                return true;

            return false;
        }
    }
}

#endif
