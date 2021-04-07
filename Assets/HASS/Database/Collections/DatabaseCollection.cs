using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HASS.Database.Templates;
using UnityEngine;

namespace HASS.Database.Collections
{
    /// <summary>
    /// Abstract Database Collection class
    /// </summary>
    public abstract class ADatabaseCollection : IDatabaseCollection
    {
#if UNITY_EDITOR
        /// <summary>
        /// The Collection Renderer to use for this collection. Customize how a collection renders in the Editor by overriding CollectionRenderer.get
        /// with a class that extends CollectionRenderer.
        /// </summary>
        private DatabaseEditor.CollectionRenderer m_CollectionRenderer;
        public virtual DatabaseEditor.CollectionRenderer CollectionRenderer
        {
            get
            {
                m_CollectionRenderer ??= new DatabaseEditor.CollectionRenderer();
                return m_CollectionRenderer;
            }
        }
#endif

        /// <summary>
        /// The Template Types this Collection supports
        /// </summary>
        private List<Type> m_TemplateTypes = null;
        public List<Type> TemplateTypes
        {
            get
            {
                return m_TemplateTypes ??= Assembly
                  .GetAssembly(this.GetType())
                  .GetTypes()
                  .Where(t => (t.IsSubclassOf(TemplateType) || t == TemplateType) && !t.IsAbstract)
                  .ToList();
            }
        }

        /// <summary>
        /// The name to display in the Editor by default uses the type as the name
        /// </summary>
        public String DisplayName { get; set; }

        /// <summary>
        /// The base parent Template type that this is collection is a collection of
        /// </summary>
        public Type TemplateType { get; set; }

        /// <summary>
        /// The physical list of database records
        /// </summary>
        public Dictionary<int, Template> DatabaseItems { get; protected set; }

        /// <summary>
        /// Start a Collection with records on creation
        /// </summary>
        public virtual Dictionary<int, Template> DefaultValues { get; set; } = null;

        public ADatabaseCollection(Type templateType)
        {
            this.TemplateType = templateType;
            this.DisplayName = Functions.ExtractNameFromType(this.GetType());
            DatabaseItems = new Dictionary<int, Template>();
        }

        /// <summary>
        /// Calls BeforeSave on each Template in the Collection
        /// Override to do something before the database is saved in the Editor
        /// </summary>
        public virtual void BeforeSave()
        {
            foreach (var template in List())
            {
                template.BeforeSave();
            }
        }

        /// <summary>
        /// Override to do something after the database is saved in the Editor
        /// </summary>
        public virtual void AfterSave()
        {
        }

        /// <summary>
        /// Gets the highest ID in the Collection + 1 
        /// </summary>
        /// <returns>The ID</returns>
        private int GetNextID()
        {
            if (DatabaseItems.Count == 0) return 0;
            return DatabaseItems.Max(item => item.Key) + 1;
        }

        /// <summary>
        /// Gets a Template by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The Template or null if not found</returns>
        public Template Get(int id)
        {
            return DatabaseItems
              .Select(item => item.Value)
              .FirstOrDefault(item => item.ID == id);
        }

        /// <summary>
        /// Gets a Template by TemplateName
        /// </summary>
        /// <param name="name">The TemplateName</param>
        /// <returns>The Template or null if not found</returns>
        public Template Get(string name)
        {
            return DatabaseItems
              .Select(item => item.Value)
              .FirstOrDefault(item => item.Name.Equals(name));
        }

        /// <summary>
        /// Removes a Template from the Collection if it exists
        /// </summary>
        /// <param name="template">The Template to remove</param>
        public void Remove(Template template)
        {
            try
            {
                var record = DatabaseItems.First(t => t.Value.ID == template.ID);
                DatabaseItems.Remove(record.Key);
            }
            catch (InvalidOperationException)
            {
                Debug.Log($"Template ID {template.ID} Not Found");
            }
        }

        /// <summary>
        /// Copys a template record
        /// </summary>
        /// <param name="toClone">The cloned template</param>
        /// <returns></returns>
        public Template Clone(Template toClone)
        {
            var template = toClone.Clone();
            template.Name = null;
            return Add(template);
        }

        /// <summary>
        /// Adds a new template of type t to the Collection
        /// </summary>
        /// <param name="t">Type must extend Template</param>
        /// <returns></returns>
        public Template Add(Type t)
        {
            return Add((Template)ScriptableObject.CreateInstance(t));
        }

        /// <summary>
        /// Adds a new template of type T to the Collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Template Add<T>() where T : Template
        {
            return Add(ScriptableObject.CreateInstance<T>());
        }

        /// <summary>
        /// Given a ScriptableObject instance, Assigns the Template ID and TemplateName
        /// and Adds it to the Collection.
        /// </summary>
        /// <param name="newTemplate">The Template ScriptableObject Instance</param>
        /// <returns>The new Template</returns>
        public Template Add(Template newTemplate)
        {
            if (newTemplate.GetType() != TemplateType && !newTemplate.GetType().IsSubclassOf(TemplateType))
            {
                throw new ArgumentException($"{newTemplate} is not of type and does not inherit from type {TemplateType}");
            }
            int id = GetNextID();
            newTemplate.ID = id;
            newTemplate.Name = newTemplate.DefaultName;
            DatabaseItems[id] = newTemplate;

            return newTemplate;
        }

        /// <summary>
        /// Gets a list of the Templates in the Collection
        /// </summary>
        /// <param name="templateType">Optionally filter only templates of a type</param>
        /// <returns>List of the Templates</returns>
        public List<Template> List(Type templateType = null)
        {
            return DatabaseItems.Select(dbitem => dbitem.Value)
              .OrderBy(template => template.ID)
              .Where(template => templateType == null || template.GetType() == templateType || template.GetType().IsSubclassOf(templateType))
              .ToList();
        }

        public virtual List<string> BuildBundle()
        {
            var builds = new List<string>();
            foreach(var item in List())
            {
                var fields = item.GetType().GetFields().Where(x => x.GetType() == typeof(DatabasePrefab)).ToList();

                foreach (var field in fields)
                {
                    var value = (DatabasePrefab)field.GetValue(item);
                    if (value != null && value.HasAsset())
                        builds.Add(value.AssetPath);
                }
            }

            return builds;
        }
    }

    /// <summary>
    /// A collection of Template records of a Type and it's children types.
    /// Usage: public class [YourCollectionName]Collection : DatabaseCollection<[YourTemplateName]Template>
    /// </summary>
    public abstract class DatabaseCollection : ADatabaseCollection
    {
        public DatabaseCollection(Type templateType) : base(templateType)
        {
        }

        public static Type GetTypeFromString(string collectionType)
        {
            return Type.GetType(collectionType);
        }
    }

    public abstract class DatabaseCollection<T> : ADatabaseCollection, IDatabaseCollection<T> where T : Template
    {
        public DatabaseCollection() : base(typeof(T))
        {
        }
    }

    /// <summary>
    /// The Interface Database Collection's must implement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDatabaseCollection<out T> : IDatabaseCollection where T : Template
    {
    }

    public interface IDatabaseCollection
    {
        public String DisplayName { get; set; }
        public Type TemplateType { get; set; }

        public List<Template> List(Type templateType = null);

        public Template Clone(Template toClone);

        public Template Add(Template newTemplate);

        public void Remove(Template template);

        public Template Get(int id);

        public Template Get(string name);

        public void BeforeSave();

        public void AfterSave();

        public List<string> BuildBundle();
    }
}