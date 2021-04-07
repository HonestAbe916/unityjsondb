using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HASS.Database.Collections;
using HASS.JSON;
using HASS.Database.Templates;
using UnityEditor;
using UnityEngine;

namespace HASS.Database
{
    /// <summary>
    /// The Main Database Class that stores the database data
    /// </summary>
    [System.Serializable]
    public class Database
    {
        /// <summary>
        /// The physical database data
        /// </summary>
        public Dictionary<Type, ADatabaseCollection> Collections { get; private set; }


        private List<Type> m_DatabaseCollectionTypes = null;
        /// <summary>
        /// Gets a list of all DatabaseCollection class types from Assembly that are not abstract
        /// </summary>
        public List<Type> DatabaseCollectionTypes
        {
            get
            {
                return m_DatabaseCollectionTypes ??= Assembly
                  .GetAssembly(typeof(ADatabaseCollection))
                  .GetTypes()
                  .Where(t => t.IsSubclassOf(typeof(ADatabaseCollection)) && !t.IsAbstract)
                  .ToList();
            }
        }

        /// <summary>
        /// The Collection that is being viewed
        /// </summary>
        public Type ViewingCollection { get; set; } = null;

        /// <summary>
        /// The Database Name
        /// </summary>
        public string Name { get; set; } = DatabaseSettings.DefaultName;

        /// <summary>
        /// Filter templates
        /// </summary>
        public string SearchQuery { get; set; } = "";

        /// <summary>
        /// Sort templates
        /// </summary>
        public SortOption SelectedSort { get; set; } = SortOption.AZ;

        public Database()
        {
            Collections = new Dictionary<Type, ADatabaseCollection>();
        }

        /// <summary>
        /// Load Database From a Text Asset
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Database Load(TextAsset json)
        {
            return (Database)JsonSerialization.Deserialize(typeof(Database), json.text);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Loads the Database using the AssetDatabase. Only use from the Editor
        /// </summary>
        /// <param name="production"></param>
        /// <returns></returns>
        public static Database EditorLoad(bool production = false)
        {
            var db = production ? DatabaseSettings.Settings.databaseName : DatabaseSettings.Settings.ViewingDatabase;
            db = DatabaseSettings.GetFullPath(db);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(db);
            if (asset == null) return null;
            return Load(asset);
        }

        /// <summary>
        /// Saves the Database to a JSON file
        /// </summary>
        /// <param name="database">The Database to save</param>
        public static void Save(Database database)
        {
            Functions.CreateFolderPath($"{DatabaseSettings.Settings.savePath}/{DatabaseSettings.Settings.databaseFolder}");
            foreach (var dc in database.ListCollections())
            {
                dc.BeforeSave();
            }


            database.Name = DatabaseSettings.Settings.ViewingDatabase;
            JsonSerialization.WriteJSON<Database>(database, DatabaseSettings.GetFullPath(database.Name));

            foreach (var dc in database.ListCollections())
            {
                dc.AfterSave();
            }
        }
#endif

        /// <summary>
        /// Get a list of all current Database Collection types
        /// </summary>
        /// <returns></returns>
        public List<Type> GetDatabaseCollections()
        {
            return Collections.Select(kvp => kvp.Key).ToList();
        }

        /// <summary>
        /// Gets the first Collection in the Database
        /// </summary>
        /// <returns>The first DatabaseCollection</returns>
        public ADatabaseCollection First()
        {
            return Collections.Select(kvp => kvp.Value).FirstOrDefault();
        }

        /// <summary>
        /// Adds a new Collection.
        /// </summary>
        /// <param name="type">The type must be of type ADatabaseCollection</param>
        /// <returns>The created Collection</returns>
        /// <exception cref="Exception"></exception>
        public ADatabaseCollection Add(Type type)
        {
            if (Collections.ContainsKey(type))
            {
                throw new Exception($"Database already contains a collection for type [{type}]");
            }

            if (!type.IsSubclassOf(typeof(ADatabaseCollection)))
            {
                throw new Exception($"[{type}] must be of type DatabaseCollection");
            }

            var newCollection = (ADatabaseCollection)Activator.CreateInstance(type);
            if (newCollection.DefaultValues != null)
            {
                foreach (var i in newCollection.DefaultValues)
                {
                    newCollection.Add(i.Value);
                }
            }

            Collections.Add(type, newCollection);
            return newCollection;
        }

        /// <summary>
        /// Remove Collection
        /// </summary>
        /// <param name="type">The Database Collection Type to remove</param>
        public void Remove(Type type)
        {
            Collections.Remove(type);
        }

        /// <summary>
        /// Gets a Database Collection By Type
        /// </summary>
        /// <param name="type">The Database Collection Type</param>
        /// <returns>The Database Collection</returns>
        public ADatabaseCollection Get(Type type)
        {
            if (type == null) throw new ArgumentNullException("A Database Collection Type must not be NULL");

            if (!type.IsSubclassOf(typeof(ADatabaseCollection)))
            {
                throw new ArgumentException($"Type {type} is not a Database Collection Type");
            }

            if (!Collections.ContainsKey(type))
            {
                throw new ArgumentOutOfRangeException($"Database does not contain a collection for type {type}");
            }

            return Collections[type];
        }

        /// <summary>
        /// Gets a Database Collection
        /// </summary>
        /// <typeparam name="T">The DatabaseCollection type</typeparam>
        /// <returns></returns>
        public ADatabaseCollection Get<T>() where T : ADatabaseCollection
        {
            return Get(typeof(T));
        }

        /// <summary>
        /// Gets a list of the current Database Collections
        /// </summary>
        /// <returns></returns>
        public List<ADatabaseCollection> ListCollections()
        {
            return Collections.Select(kvp => kvp.Value).ToList();
        }

        /// <summary>
        /// Gets a template record using Record Object
        /// </summary>
        /// <param name="recordID">The Record Object</param>
        /// <returns>The referenced template record or null if not found</returns>
        public Template GetTemplate(ARecord recordID)
        {
            if (!Collections.ContainsKey(recordID.Collection))
            {
                throw new ArgumentOutOfRangeException($"Database does not contain a collection for type {recordID.Collection}");
            }

            return Collections[recordID.Collection].Get(recordID.ID);
        }
    }
}