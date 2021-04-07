# if UNITY_EDITOR

using HASS.Database.DatabaseEditor;
using UnityEngine;
namespace HASS.Example
{
    /// <summary>
    /// Overrides the default renderer to add a block preview.
    /// </summary>
    public class BlockCollectionRenderer : CollectionRenderer
    {
        private static GameObject _preview;
        private static GameObject Preview
        {
            get
            {
                if (_preview == null)
                    _preview = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HASS/Prefabs/Preview.prefab");

                return _preview;
            }
        }

        public override void Render()
        {
            base.Render();
            var active = DatabaseWindow.ActiveTemplateID;
            if (active == -1) return;
            var template = (BlockTemplate)DatabaseWindow.ActiveCollection.Get(active);
            if (template == null) return;

            var previewID = $"ID [{active}] Color [{template.color.ToString()}]";

            if (!DatabaseWindow.PreviewChanged(previewID))
                return;

            Preview.GetComponent<MeshRenderer>().sharedMaterial.color = template.color;
            DatabaseWindow.SetPreview(Preview, previewID);
        }
    }
}

# endif
