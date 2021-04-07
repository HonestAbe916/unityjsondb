# if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using HASS.Database.Collections;
using HASS.Database.Templates;
using NUnit.Framework;

namespace HASS.Database.DatabaseEditor
{
    /// <summary>
    /// Renders the database editor window
    /// </summary>
    public class DatabaseWindow : EditorWindow
    {
        /// <summary>
        /// The Database we are editing
        /// </summary>
        private Database m_Database;
        public Database Database
        {
            get => m_Database;
            set
            {
                m_Database = value;

                if (m_Database != null && m_Database.ViewingCollection != null)
                {
                    ActiveCollection = m_Database.Get(m_Database.ViewingCollection);
                }
                else
                {
                    m_ActiveCollection = null;
                    m_ActiveRenderer = null;
                }

                m_AddTemplateIndex = 0;
            }
        }

        /// <summary>
        /// The DatabaseCollection being viewed
        /// </summary>
        private int m_ViewActiveCollectionIndex = 0;
        private ADatabaseCollection m_ActiveCollection;
        private CollectionRenderer m_ActiveRenderer;
        public ADatabaseCollection ActiveCollection
        {
            get => m_ActiveCollection;
            private set
            {
                if (value == null)
                {
                    ClearPreview();
                    m_ActiveCollection = null;
                    m_ActiveRenderer = null;
                    m_AddTemplateIndex = 0;
                    Database.ViewingCollection = null;
                    m_ViewActiveCollectionIndex = 0;
                    return;
                }

                if (m_ActiveCollection != null && m_ActiveCollection.GetType() == value.GetType()) return;

                ClearPreview();
                m_ActiveCollection = value;
                m_ActiveRenderer = m_ActiveCollection.CollectionRenderer;
                m_ActiveRenderer.Init(this);
                m_AddTemplateIndex = 0;
                Database.ViewingCollection = m_ActiveCollection.GetType();
                var list = Database.GetDatabaseCollections();
                var index = list.FindIndex(t => t == Database.ViewingCollection);
                m_ViewActiveCollectionIndex = index;
            }
        }

        /// <summary>
        /// The Template being viewed
        /// </summary>
        public int ActiveTemplateID { get; private set; } = -1;

        /// <summary>
        /// For previewing GameObjects, needs to be reinstantiated every time the GameObject
        /// </summary>
        private UnityEditor.Editor _gameObjectEditor = null;
        private UnityEditor.Editor GameObjectEditor
        {
            get => _gameObjectEditor;
            set
            {
                if (_gameObjectEditor != null)
                {
                    DestroyImmediate(_gameObjectEditor);
                }

                _gameObjectEditor = value;
            }
        }
        private GameObject PreviewGameObject { get; set; } = null;

        /// <summary>
        /// Unique Identifier of the GameObject we are previewing, so to know when to re init the GameObjectEditor
        /// </summary>
        private String PreviewID { get; set; } = null;

        /// <summary>
        /// Live Editor Fields placeholders
        /// </summary>
        private Vector2 m_ScrollPos;
        private int m_AddTemplateIndex = 0;
        private int m_AddNewCollectionIndex = 0;
        private string fileName = DatabaseSettings.DefaultName;

        /// <summary>
        /// Switch used to save the Database on delay
        /// </summary>
        public bool ShouldSave { get; set; } = false;

        private GUIStyle m_RichTextLabelStyle = null;
        public GUIStyle RichTextLabelStyle
        {
            get
            {
                return m_RichTextLabelStyle ??= new GUIStyle(EditorStyles.label)
                {
                    richText = true
                };

            }
        }


        private void Awake()
        {
            fileName = DatabaseSettings.Settings.ViewingDatabase;
            Load();
        }


        public void Save()
        {
            Database.Save(Database);
        }

        private void Load()
        {
            Database = EditorAssetLoader.GetDB();
        }

        private void Update()
        {
            if (ShouldSave)
            {
                ShouldSave = false;
                Save();
            }
        }

        private void OnGUI()
        {
            TopMenu();
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.ExpandHeight(true));
            RenderDatabase();
            EditorGUILayout.EndScrollView();
            if (ActiveCollection != null) m_ActiveRenderer.AfterOnGUI();
            if (GameObjectEditor != null)
            {
                var activeTemplate = ActiveCollection.Get(ActiveTemplateID);
                if (activeTemplate == null) return;
                //GameObjectEditor.DrawHeader();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal(EditorStyles.toolbar);



                var baseLabel = $"{activeTemplate.Name} Preview <size=10><color={Style.GetHexFromColor(Style.Secondary)}>({PreviewID})</color></size>";

                GUILayout.Label(baseLabel, new GUIStyle(EditorStyles.label) { richText = true });
                GUILayout.EndHorizontal();
                GameObjectEditor.OnPreviewGUI(
                  GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 200),
                  EditorStyles.whiteLabel);
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Renders the entire header section of the Database Window
        /// </summary>
        private void TopMenu()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            RenderDatabaseBar();

            Style.VerticalSpace();

            GUILayout.Label("Collection", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            RenderViewCollectionBar();
            GUILayout.EndHorizontal();

            Style.VerticalSpace();

            if (ActiveCollection != null)
                RenderActiveCollectionBar();

            GUILayout.EndVertical();
            Style.VerticalSpace();
        }

        /// <summary>
        /// Calls ActiveCollection.CollectionRenderer.Render() that renders the physical collection
        /// </summary>
        private void RenderDatabase()
        {
            if (ActiveCollection == null) return;
            m_ActiveRenderer.Render();
        }

        /// <summary>
        /// Renders Database fileName field + save/load
        /// </summary>
        private void RenderDatabaseBar()
        {
            GUILayout.Label("Database", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            fileName = EditorGUILayout.TextField(fileName, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

            if (Style.ToolBarButton("Save"))
            {
                DatabaseSettings.Settings.ViewingDatabase = fileName;
                DatabaseSettings.Settings = DatabaseSettings.Settings; // save editor prefs
                Save();
            }

            if (Style.ToolBarButton("Load"))
            {
                DatabaseSettings.Settings.ViewingDatabase = fileName;
                DatabaseSettings.Settings = DatabaseSettings.Settings; // save editor prefs
                Load();
            }

            if (Style.ToolBarButton("Clear"))
            {
                Database = new Database();
            }

            RenderCreateCollectionBar();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders Create new Collection Form
        /// </summary>
        public void RenderCreateCollectionBar()
        {
            var listFullNames = GetInactiveCollectionTypes(true).ToArray();
            if (listFullNames.Length == 0) return;

            //EditorGUIUtility.IconContent("Toolbar Plus")
            if (!EditorGUILayout.DropdownButton(new GUIContent("Add Collection"), FocusType.Passive, GUILayout.MaxWidth(Style.XL)))
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            foreach (var collection in listFullNames)
            {
                var selectedType = listFullNames.ElementAtOrDefault(m_AddNewCollectionIndex);
                menu.AddItem(new GUIContent(Functions.ExtractNameFromType(collection)), false, () => HandleItemClicked(selectedType));
            }
            menu.ShowAsContext();
        }

        void HandleItemClicked(string selectedType)
        {
            if (!String.IsNullOrEmpty(selectedType))
                ActiveCollection = Database.Add(DatabaseCollection.GetTypeFromString(selectedType));
        }

        /// <summary>
        /// Renders the view collection dropdown
        /// </summary>
        private void RenderViewCollectionBar()
        {
            var currentCollections = GetCurrentCollectionTypes(false);
            if (currentCollections.Length == 0) return;

            Style.Label("View");
            m_ViewActiveCollectionIndex = EditorGUILayout.Popup(m_ViewActiveCollectionIndex, currentCollections, EditorStyles.toolbarPopup);

            var list = GetCurrentCollectionTypes(true);
            Database.ViewingCollection = list.ElementAtOrDefault(m_ViewActiveCollectionIndex) != null ?
              DatabaseCollection.GetTypeFromString(list[m_ViewActiveCollectionIndex]) :
              null;

            ActiveCollection = Database.Get(Database.ViewingCollection);
        }

        /// <summary>
        /// Renders the Add template form, sort dropdown, and search field
        /// </summary>
        private void RenderActiveCollectionBar()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            var label = Style.RichTextLabelType(Functions.ExtractNameFromType(ActiveCollection.GetType()), ActiveCollection.TemplateType);
            GUILayout.Label(label, RichTextLabelStyle);
            if (Style.ToolBarButton("Delete", Status.Danger))
            {
                Database.Remove(Database.ViewingCollection);
                Database.ViewingCollection = null;
                ActiveCollection = null;
                GUILayout.EndHorizontal();
                return;
            }
            GUILayout.EndHorizontal();


            GUILayout.Space(5f);
            RenderAddTemplateBar();
            GUILayout.Space(5f);
            RenderSearchAndSortBar();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Renders Add template form
        /// </summary>
        private void RenderAddTemplateBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            Style.Label("Template");
            m_AddTemplateIndex = EditorGUILayout.Popup(
              m_AddTemplateIndex,
              GetActiveCollectionTemplateTypes(false), EditorStyles.toolbarPopup);

            if (m_AddTemplateIndex == -1)
            {
                GUILayout.EndHorizontal();
                return;
            }
            if (Style.ToolBarButton("Add"))
            {
                var childType = GetActiveCollectionTemplateTypes(true)[m_AddTemplateIndex];
                var template = ActiveCollection.Add(Type.GetType(childType));
                SetActiveRecord(template.ID);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders search/sort
        /// </summary>
        private void RenderSearchAndSortBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            //GUILayout.Label("Search + Sort");
            Style.Label("Search");
            Database.SearchQuery = EditorGUILayout.TextField(Database.SearchQuery, EditorStyles.toolbarSearchField);
            Style.Label("Sort");
            Database.SelectedSort = (SortOption)EditorGUILayout.EnumPopup(Database.SelectedSort, EditorStyles.toolbarPopup, GUILayout.MaxWidth(Style.XS));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Sets the template show property to true on the active template and false to other templates
        /// </summary>
        /// <param name="activeId"></param>
        public void SetActiveRecord(int activeId)
        {
            if (ActiveTemplateID != activeId || activeId == -1)
            {
                ClearPreview();
            }

            ActiveTemplateID = activeId;

            foreach (var item in ActiveCollection.List())
            {
                item.Show = (item.ID == ActiveTemplateID);
            }
        }

        /// <summary>
        /// Preview a GameObject in the Editor
        /// </summary>
        /// <param name="objToPreview"></param>
        /// <param name="previewID">A Unique ID to tell the GameObjectEditor when it needs to Rebuild the preview
        /// Generally, this can just be the Template.id but if modifications are made to the GameObject (like a color picker)
        /// and you want to tell it to Rerender to show those changes then you need to make sure the PreviewID changes even
        /// though we are still previewing the same GameObject i.e $"{Template.id}-{ChangesYouMade.ToString()}"
        /// </param>
        public void SetPreview(GameObject objToPreview, String previewID)
        {
            if (!PreviewChanged(previewID)) return;

            PreviewGameObject = objToPreview;
            PreviewID = previewID;
            GameObjectEditor = UnityEditor.Editor.CreateEditor(PreviewGameObject);
            //GameObjectEditor.DrawHeader();
        }

        /// <summary>
        /// Clears the preview gameObject
        /// </summary>
        public void ClearPreview()
        {
            PreviewGameObject = null;
            GameObjectEditor = null;
            PreviewID = null;
        }

        /// <summary>
        /// Did the Preview ID change
        /// </summary>
        /// <param name="previewID"></param>
        /// <returns></returns>
        public bool PreviewChanged(string previewID)
        {
            return previewID != PreviewID;
        }

        /// <summary>
        /// Gets a list of DatabaseCollection types as strings that are currently in the Database
        /// </summary>
        /// <param name="fullName">Include Namespace</param>
        /// <returns></returns>
        private string[] GetCurrentCollectionTypes(bool fullName = true)
        {
            return Database.GetDatabaseCollections()
              .Select(type => fullName ? type.FullName : type.Name)
              .ToArray();
        }

        /// <summary>
        /// Gets a list of DatabaseCollection types as strings that are currently NOT in the Database
        /// </summary>
        /// <param name="fullName">Include Namespace</param>
        /// <returns></returns>
        private string[] GetInactiveCollectionTypes(bool fullName = true)
        {
            var list = Database.DatabaseCollectionTypes;
            var current = Database.GetDatabaseCollections();

            return list.Where(t => !current.Contains(t))
              .Select(type => fullName ? type.FullName : type.Name)
              .ToArray();
        }

        /// <summary>
        /// Gets a list of Template types as strings that the current Database Collection supports.
        /// </summary>
        /// <param name="fullName">Include Namespace</param>
        /// <returns></returns>
        private string[] GetActiveCollectionTemplateTypes(bool fullName = true)
        {
            return ActiveCollection.TemplateTypes
              .Select(type => fullName ? type.FullName : type.Name)
              .ToArray();
        }

        /// <summary>
        /// Auto Reload methods
        /// </summary>
        private void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        public void OnBeforeAssemblyReload()
        {
            if (DatabaseSettings.Settings.liveReload)
            {
                Save();
            }
        }

        public void OnAfterAssemblyReload()
        {
            if (DatabaseSettings.Settings.liveReload)
            {
                Load();
            }
        }
    }
}

#endif