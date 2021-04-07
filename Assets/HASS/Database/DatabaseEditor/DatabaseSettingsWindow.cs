#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using HASS.JSON;
using UnityEditor;

namespace HASS.Database.DatabaseEditor
{
    /// <summary>
    /// Renders The Database Settings Window
    /// </summary>
    public class DatabaseSettingsWindow : EditorWindow
    {
        /// <summary>
        /// Renders the DatabaseSettings fields
        /// </summary>
        private void OnGUI()
        {
            var serializedObject = new UnityEditor.SerializedObject(DatabaseSettings.Settings);
            //DatabaseSettings.Settings.ViewingDatabase = EditorGUILayout.TextField("Viewing Database", DatabaseSettings.Settings.ViewingDatabase);
            foreach (FieldInfo field in typeof(DatabaseSettings).GetFields().Where(x => !x.IsPrivate).ToList())
            {
                var serializedField = serializedObject.FindProperty(field.Name);
                if (serializedField != null)
                    EditorGUILayout.PropertyField(serializedField);
            }

            serializedObject.ApplyModifiedProperties();

            EditorPrefs.SetString(
              DatabaseSettings.EditorPrefsKey,
              JsonSerialization.Serialize(typeof(DatabaseSettings), DatabaseSettings.Settings)
            );
        }
    }
}

#endif
