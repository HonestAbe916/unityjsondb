using System;
using HASS.JSON;
using UnityEngine;

namespace HASS.Database
{
    /// <summary>
    /// For the Database Settings window. Stores users settings in EditorPrefs.
    /// </summary>
    public class DatabaseSettings : ScriptableObject
    {
        public const string EditorPrefsKey = "DatabaseSettingsJSON";
        public const string DefaultFolder = "Databases";
        public const string DefaultName = "default";
        public const string DefaultPath = "Assets/Resources";

        /// <summary>
        /// Should the database window auto reload after assembly changes.
        /// </summary>
        public bool liveReload;

        /// <summary>
        /// The production database name
        /// </summary>
        public string databaseName;

        /// <summary>
        /// The folder to save databases
        /// </summary>
        public string databaseFolder;

        /// <summary>
        /// The folder path to save databases
        /// </summary>
        public string savePath;

        /// <summary>
        /// The active database that the Editor is viewing
        /// </summary>
        public string ViewingDatabase { get; set; } = DefaultName;

#if UNITY_EDITOR
        private static DatabaseSettings _databaseSettings = null;
        public static DatabaseSettings Settings
        {
            set
            {
                _databaseSettings = value;
                if (_databaseSettings != null)
                {
                    UnityEditor.EditorPrefs.SetString(
                      EditorPrefsKey,
                      JsonSerialization.Serialize(typeof(DatabaseSettings), _databaseSettings)
                    );
                }
            }
            get
            {
                if (_databaseSettings == null)
                {
                    _databaseSettings = GetDatabaseSettings();
                }

                return _databaseSettings;
            }
        }

        public static DatabaseSettings GetDatabaseSettings()
        {
            DatabaseSettings settings;
            var jsonString = UnityEditor.EditorPrefs.GetString(EditorPrefsKey);
            if (!String.IsNullOrEmpty(jsonString))
                settings = (DatabaseSettings)JsonSerialization.Deserialize(typeof(DatabaseSettings), jsonString);
            else
            {
                settings = ScriptableObject.CreateInstance<DatabaseSettings>();
                settings.liveReload = true;
                settings.databaseFolder = DefaultFolder;
                settings.databaseName = DefaultName;
                settings.savePath = DefaultPath;
                settings.ViewingDatabase = DefaultName;
            }

            return settings;
        }

        public static string GetFullPath(string filename = DefaultName)
        {
            return $"{DatabaseSettings.Settings.savePath}/{DatabaseSettings.Settings.databaseFolder}/{filename}.json";
        }
#endif
    }
}
