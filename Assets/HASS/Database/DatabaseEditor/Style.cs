#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HASS.Database.DatabaseEditor
{
    public static class Style
    {
        public const int DefaultSpacing = 15;

        public const float LabelWidth = 200f;

        public const float XXS = 35f;
        public const float XS = 60f;
        public const float MD = 70f;
        public const float LG = 85f;
        public const float XL = 125f;
        public const float XXL = 175f;
        public const float XXXL = 250f;

        public const char MenuIcon = '\u2630';
        public const char TemplateIcon = '\u1E6E';

        public static Color Danger = new Color(0.96f, 0f, 0.34f);
        public static Color Warning = new Color(0.82f, 0.86f, 0.14f);
        public static Color Info = new Color(0f, 0.48f, 1f);
        public static Color Normal = new Color(1f, 1f, 1f);
        public static Color Success = new Color(0.16f, 0.65f, 0.58f);
        public static Color Secondary = new Color(0.42f, 0.46f, 0.49f);

        public static Color GetColor(Status status)
        {
            switch (status)
            {
                case Status.Danger: return Danger;
                case Status.Info: return Info;
                case Status.Success: return Success;
                case Status.Warning: return Warning;
                case Status.Normal: return Normal;
                case Status.Secondary: return Secondary;
                default: return Normal;
            }
        }

        public static string Icon(char icon)
        {
            return $"<color={GetHexFromColor(Secondary)}>{icon.ToString()}</color>";
        }

        public static string RichTextLabelType(string rawFieldName, Type collectionType = null, Type templateType = null)
        {
            var color = GetHexFromColor(Secondary);
            var baseLabel = ObjectNames.NicifyVariableName(rawFieldName);
            var collectionTypeLabel = collectionType != null ? $"{Functions.ExtractNameFromType(collectionType, true)}" : "";
            var templateTypeLabel = templateType != null ? $"{Functions.ExtractNameFromType(templateType, true)}" : "";

            if (collectionType != null && templateType != null)
            {
                return $"{baseLabel} <size=10><<color={color}>{collectionTypeLabel}.{templateTypeLabel}</color>></size>";
            }

            return $"{baseLabel} <size=10><<color={color}>{collectionTypeLabel}{templateTypeLabel}</color>></size>";
        }

        public static string GetHexFromColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }

        public static bool ToolBarButton(string label, Status status = Status.Normal, float width = Style.XS)
        {
            var style = new GUIStyle(EditorStyles.toolbarButton)
            {
                normal =
                {
                    textColor = Style.GetColor(status)
                }
            };
            return GUILayout.Button(label.ToUpper(), style, GUILayout.MaxWidth(width));
        }

        public static void Label(string label, Status status = Status.Normal, float width = Style.XS)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                  textColor = Style.GetColor(status)
                }
            };
            GUILayout.Label(label, style, GUILayout.MaxWidth(width));
        }

        /// <summary>
        /// Adds a Vertical Space
        /// </summary>
        /// <param name="amount"></param>
        public static void VerticalSpace(float amount = DefaultSpacing)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(amount);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Adds a space
        /// </summary>
        /// <param name="scale"></param>
        public static void Spacing(float scale = 1f)
        {
            GUILayout.Space(EditorGUI.indentLevel * scale * DefaultSpacing);
        }
    }
}
#endif
