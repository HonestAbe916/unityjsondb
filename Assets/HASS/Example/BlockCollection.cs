using HASS.Database.Collections;

namespace HASS.Example
{
    /// <summary>
    /// Example Database Collection with custom collection renderer
    /// </summary>
    public class BlockCollection : DatabaseCollection<BlockTemplate>
    {
#if UNITY_EDITOR
        private Database.DatabaseEditor.CollectionRenderer m_CollectionRenderer;
        public override Database.DatabaseEditor.CollectionRenderer CollectionRenderer
        {
            get
            {
                m_CollectionRenderer ??= new BlockCollectionRenderer();
                return m_CollectionRenderer;
            }
        }
#endif
    }
}
