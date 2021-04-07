# if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using HASS.Database.Templates;
using UnityEditor;
using UnityEngine;

namespace HASS.Database.DatabaseEditor.Drawers
{
    [CustomPropertyDrawer(typeof(DatabasePrefab), true)]
    public class DatabasePrefabDrawer : PropertyDrawer
    {
        private int _index = 0;

        private DatabasePrefab RenderOne(Rect position, SerializedProperty property, DatabasePrefab databasePrefab, Template template)
        {
            UnityEngine.Object selectedObject = null;
            if (!String.IsNullOrEmpty(databasePrefab.AssetPath) && databasePrefab.AssetType != null)
            {
                selectedObject = AssetDatabase.LoadAssetAtPath(databasePrefab.AssetPath, databasePrefab.AssetType);
            }

            selectedObject = EditorGUI.ObjectField(position, property.name, selectedObject, typeof(UnityEngine.Object), false);

            if (selectedObject != null)
            {
                databasePrefab.AssetPath = AssetDatabase.GetAssetPath(selectedObject);
                databasePrefab.AssetType = selectedObject.GetType();
                databasePrefab.ID = AssetDatabase.AssetPathToGUID(databasePrefab.AssetPath);
            }

            return databasePrefab;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var template = (Template)property.serializedObject.targetObject;
            var parentObject = GetParentObject(property, template);
            var currentValue = fieldInfo.GetValue(parentObject);


            if (currentValue.GetType().IsAssignableFrom(typeof(List<DatabasePrefab>)))
            {
                var list = (List<DatabasePrefab>)currentValue;
                var item = list[_index];
                item = RenderOne(position, property, item, template);
                list[_index] = item;

                fieldInfo.SetValue(template, list);

                _index += 1;
                if (_index >= list.Count) _index = 0;
            }
            else if (currentValue.GetType().IsAssignableFrom(typeof(DatabasePrefab[])))
            {
                var array = (DatabasePrefab[])currentValue;
                var list = array.ToList();
                var item = list[_index];
                item = RenderOne(position, property, item, template);
                list[_index] = item;

                fieldInfo.SetValue(template, list.ToArray());

                _index += 1;
                if (_index >= list.Count) _index = 0;
            }
            else
            {
                var convertedValue = (DatabasePrefab)currentValue;
                convertedValue = RenderOne(position, property, convertedValue, template);
                fieldInfo.SetValue(parentObject, convertedValue);
            }

            // might need some sort of index rest ? detect if new property started for lists?

            EditorGUI.EndProperty();
        }

        public static System.Object GetParentObject(SerializedProperty property, System.Object value)
        {
            var path = property.propertyPath.Split('.');

            System.Object theObject = value;
            System.Object parentObject = null;

            for (int i = 0; i < path.Length; i++)
            {
                parentObject = theObject;
                var theFields = parentObject.GetType().GetFields();
                var currentPath = path[i];

                // hack: if field is not found then it's a regular object path of the defualt parent and propertyPath returns some girbberish :shrug:
                var theField = theFields.FirstOrDefault(f => f.Name == currentPath);
                if (theField == null)
                {
                    return property.serializedObject.targetObject;
                }

                theObject = theField.GetValue(parentObject);
            }

            return parentObject;
        }
    }
}

#endif
