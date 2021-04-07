# if UNITY_EDITOR

using UnityEditor;

namespace HASS.Database.DatabaseEditor
{
    /// <summary>
    /// Renders the Database top bar menu
    /// </summary>
    public static class Menu
    {
        [MenuItem("Database/Open")]
        public static void ShowDatabaseWindow()
        {
            EditorWindow.GetWindow(typeof(DatabaseWindow));
        }

        [MenuItem("Database/Settings")]
        public static void ShowDatabaseSettings()
        {
            EditorWindow.GetWindow(typeof(DatabaseSettingsWindow));
        }
    }
}

#endif
