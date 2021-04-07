# if UNITY_EDITOR

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HASS.Database.DatabaseEditor.Drawers
{
    [CustomPropertyDrawer(typeof(ARecord), true)]
    public class RecordDrawer : PropertyDrawer
    {
        private int _index = 0;
        private GUIStyle style = EditorStyles.popup;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var template = property.serializedObject.targetObject;
            var parentObject = GetParentObject(property, template);
            var currentValue = fieldInfo.GetValue(parentObject);

            if (currentValue is IEnumerable enumerable)
            {
                var iterator = enumerable.GetEnumerator();
                var currentIndex = -1;
                while (currentIndex < _index)
                {
                    bool next = iterator.MoveNext();
                    if (!next)
                    {
                        _index = 0;
                        iterator.Reset();
                        iterator.MoveNext();
                        break;
                    }
                    currentIndex += 1;
                }

                var item = (IRecord)iterator.Current;
                RenderOne(position, property, item, parentObject);

                _index += 1;
            }
            else
            {
                var convertedValue = (IRecord)currentValue;
                convertedValue = RenderOne(position, property, convertedValue, parentObject);
                fieldInfo.SetValue(parentObject, convertedValue);
            }

            EditorGUI.EndProperty();
        }

        private IRecord RenderOne(Rect position, SerializedProperty property, IRecord databaseId, object parentObject)
        {
            if (databaseId.Collection == null)
            {
                throw new Exception($"{fieldInfo.Name} Collection property is NULL");
            }

            Type collectionType = databaseId.Collection;
            Type filterType = databaseId.Filter;

            // if in the Database Window, get currently viewing database, otherwise get production db
            var useProductionDB = parentObject.GetType().IsSubclassOf(typeof(MonoBehaviour));
            Database db = EditorAssetLoader.GetDB(useProductionDB);

            var collection = db.Get(collectionType);
            if (collection == null)
            {
                Debug.Log($"Skipping {fieldInfo.Name} because Collection {collectionType} was not found.");
                return databaseId;
            }

            var templates = collection.List(filterType);

            var options = templates
              .Select(t => $"[{t.ID}] {t.Name} <{Functions.ExtractNameFromType(t.GetType(), true)}>")
              .ToArray();

            var missingKey = $"Missing ID {databaseId.ID}";
            var selectedIndex = templates.FindIndex(t => t.ID == databaseId.ID);

            // could not find index in selection dropdown?
            if (selectedIndex == -1 && databaseId.ID != -1)
            {
                var updatedList = options.ToList();
                updatedList.Add(missingKey);
                options = updatedList.ToArray();
                selectedIndex = options.Length - 1;
            }
            else
            {
                selectedIndex = selectedIndex == -1 ? 0 : selectedIndex;
            }

            var inputPos = new Rect(position);
            var labelPos = new Rect(position)
            {
                width = Style.LabelWidth
            };


            inputPos.x += labelPos.width - 15f;
            inputPos.width -= inputPos.x - 7f;

            EditorGUI.LabelField(labelPos, Style.RichTextLabelType(property.name, collectionType, filterType), new GUIStyle(EditorStyles.label) { richText = true });
            selectedIndex = EditorGUI.Popup(inputPos, "", selectedIndex, options);


            if (!options[selectedIndex].Equals(missingKey))
            {
                var templateID = Int32.Parse(options[selectedIndex].Split('[', ']')[1]);
                databaseId.ID = templateID;
            }

            databaseId.Collection = collectionType;
            databaseId.Filter = filterType;
            return databaseId;
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

                // hack: if field is not found then it's a regular object path of the defualt parent and propertyPath returns some girbberish
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
