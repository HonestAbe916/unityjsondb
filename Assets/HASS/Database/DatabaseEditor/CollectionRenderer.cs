# if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using HASS.Database.Templates;

namespace HASS.Database.DatabaseEditor
{
    /// <summary>
    /// Renders The Template Records of a DatabaseCollection
    /// Extend this class and override the DatabaseCollection.CollectionRenderer to customize how it renders
    /// </summary>
    public class CollectionRenderer
    {
        protected DatabaseWindow DatabaseWindow;
        protected string lastLabel = "";
        private SaveDelay Saver;

        public virtual void Init(DatabaseWindow databaseWindow)
        {
            DatabaseWindow = databaseWindow;
            Saver = new SaveDelay(2f, 0.2f, () => DatabaseWindow.ShouldSave = true);
        }

        /// <summary>
        /// Gets the templates for the current collection
        /// while taking into account the selected sort and search 
        /// </summary>
        /// <returns></returns>
        protected virtual List<Template> GetCollectionTemplates()
        {
            var db = EditorAssetLoader.GetDB();
            var collection = DatabaseWindow.ActiveCollection;
            var list = db.Get(collection.GetType()).List();

            switch (db.SelectedSort)
            {
                case SortOption.AZ:
                    {
                        list = list.OrderBy(x => x.ID).ToList();
                        break;
                    }
                case SortOption.ZA:
                    {
                        list = list.OrderByDescending(x => x.ID).ToList();
                        break;
                    }
                case SortOption.TYPES:
                    {
                        list = list.OrderBy(x => x.GetType().ToString()).ToList();
                        break;
                    }
                default: break;
            }

            if (!String.IsNullOrEmpty(db.SearchQuery))
            {
                list = list.Where(x => x.Name.IndexOf(db.SearchQuery, 0, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
            }

            return list;
        }

        /// <summary>
        /// Renders each Template
        /// </summary>
        public virtual void Render()
        {
            var list = GetCollectionTemplates();

            if (list.Count == 0)
            {
                GUILayout.Label($"<b>No Records to Show</b>", new GUIStyle(EditorStyles.label) { richText = true });
                return;
            }

            foreach (Template item in list)
            {
                RenderItemHeader(item);

                if (item.ID == DatabaseWindow.ActiveTemplateID)
                {
                    RenderItem(item);
                }
            }
        }

        /// <summary>
        /// Renders the template Header
        /// </summary>
        /// <param name="template"></param>
        protected virtual void RenderItemHeader(Template template)
        {
            var db = EditorAssetLoader.GetDB();
            var currentLabel = Functions.ExtractNameFromType(template.GetType());
            if (currentLabel != lastLabel && db.SelectedSort == SortOption.TYPES)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(currentLabel, EditorStyles.boldLabel);
                Style.Spacing();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            bool prevValue = template.Show;

            var label = Style.RichTextLabelType(template.Name, null, template.GetType());
            template.Show = EditorGUILayout.Foldout(
              template.Show,
              $"<color={Style.GetHexFromColor(Style.Secondary)}>{template.ID}</color> {label}",
              true,
              new GUIStyle(EditorStyles.foldout) { richText = true });

            if (template.Show != prevValue)
            {
                DatabaseWindow.SetActiveRecord(template.Show ? template.ID : -1);
            }


            lastLabel = currentLabel;
            if (Style.ToolBarButton("Clone", Status.Secondary))
            {
                var newTemplate = DatabaseWindow.ActiveCollection.Clone(template);
                DatabaseWindow.SetActiveRecord(newTemplate.ID);
            }
            if (Style.ToolBarButton("Delete", Status.Danger))
            {
                DatabaseWindow.ActiveCollection.Remove(template);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders the template body
        /// </summary>
        /// <param name="template"></param>
        protected virtual void RenderItem(Template template)
        {
            EditorGUILayout.BeginVertical("box");
            RenderTemplate(template);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Renders the template body
        /// </summary>
        /// <param name="template"></param>
        protected virtual Template RenderTemplate(Template template, bool showLabel = true)
        {
            if (showLabel)
            {
                EditorGUILayout.BeginHorizontal();
                Style.Spacing();
                GUILayout.Label($"{Functions.ExtractNameFromType(template.GetType())}");
                EditorGUILayout.EndHorizontal();
            }

            if (template == null)
            {
                Debug.Log("template is NULL");
                return null;
            }

            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;

            var currentName = template.Name;
            SerializedObject serializedObject = new UnityEditor.SerializedObject(template);
            RenderFields(serializedObject, template);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            if (serializedObject.hasModifiedProperties || !currentName.Equals(template.Name))
            {
                serializedObject.ApplyModifiedProperties();
                template.OnUpdate();
                //DatabaseWindow.Save();
                Saver.Save();
            }

            return template;
        }

        /// <summary>
        /// Only gets fields not properties, because unity's serializedObject.FindProperty(field.Name) doesn't work on properties.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="template"></param>
        protected virtual void RenderFields(SerializedObject serializedObject, Template template)
        {
            var list = template.GetType().GetFields()
              .Where(x => !x.IsPrivate && x.GetCustomAttribute<HideInInspector>() == null)
              .ToList();

            EditorGUIUtility.labelWidth = Style.LabelWidth;
            template.Name = EditorGUILayout.TextField("Name", template.Name);

            foreach (FieldInfo field in list)
            {
                RenderField(serializedObject, field, template);
            }
        }

        /// <summary>
        /// Uses unity's property field by default to Render the Field. You can override this to customize how a collection renders a field.
        /// However, you are much better of using unity's custom drawer as it will work on children objects and lists and arrays automagically and
        /// unity's PropertyField() will respect any custom drawers you create.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="field"></param>
        /// <param name="theRecord"></param>
        protected virtual void RenderField(SerializedObject serializedObject, FieldInfo field, System.Object theRecord)
        {
            EditorGUIUtility.labelWidth = Style.LabelWidth;
            SerializedProperty serializedField = serializedObject.FindProperty(field.Name);
            if (serializedField == null)
            {
                Debug.Log($"CollectionRenderer -> Skipping {field.Name} because serializedField is NULL");
                return;
            }
            EditorGUILayout.PropertyField(serializedField);
        }

        /// <summary>
        /// Override to Add something after the main gui finishes rendering the frame.
        /// </summary>
        public virtual void AfterOnGUI()
        {
        }
    }
}

#endif
