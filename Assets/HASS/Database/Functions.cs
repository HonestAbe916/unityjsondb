using System;
using System.Linq;

namespace HASS.Database
{
    /// <summary>
    /// Utility functions
    /// </summary>
    public static class Functions
    {
        public static string ExtractNameFromType(string type, bool replaceSuffix = false)
        {
            var name = type.Split('.').ToList().Last();
            if (replaceSuffix)
                name = name.Replace("Template", "").Replace("Collection", "");

            return name;
        }

        public static string ExtractNameFromType(Type type, bool replaceSuffix = false)
        {
            return ExtractNameFromType(type.ToString(), replaceSuffix);
        }

        public static void CreateFolderPath(string path)
        {
#if UNITY_EDITOR
            var paths = path.Split('/');
            var currentPath = "";

            foreach (var p in paths)
            {
                var parentPath = currentPath;
                currentPath = currentPath.Equals("") ? p : $"{currentPath}/{p}";
                //Debug.Log($"{parentPath} - {currentPath}");
                if (!UnityEditor.AssetDatabase.IsValidFolder(currentPath))
                {
                    UnityEditor.AssetDatabase.CreateFolder(parentPath, p);
                }
            }
#endif
        }
    }

}