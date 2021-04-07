using HASS.Database;
using UnityEngine;
namespace HASS.Example
{
    /// <summary>
    /// Example, spawns blocks based on what's specified in the template
    /// </summary>
    public class SpawnerComponent : MonoBehaviour
    {
        public GameObject blockPrefab;

        public Record<SpawnerCollection> spawner;

        private SpawnerTemplate m_Template;
        private float m_Timer;

        private void Start()
        {
            m_Template = (SpawnerTemplate)DatabaseComponent.DB.GetTemplate(spawner);
            m_Timer = m_Template.spawnTime;
        }

        private void Update()
        {
            m_Timer -= Time.deltaTime;
            if (m_Timer <= 0)
            {
                m_Timer = m_Template.spawnTime;
                Spawn();
            }
        }

        private Vector3 GetRandomPos()
        {
            var pos = transform.position;
            pos.z += Random.Range(-3f, 3f);
            pos.x += Random.Range(-3f, 3f);
            return pos;
        }

        public void Spawn()
        {
            var selectedBlockRecordID = m_Template.blockTypes[Random.Range(0, m_Template.blockTypes.Count)];
            var blockTemplate = (BlockTemplate)DatabaseComponent.DB.GetTemplate(selectedBlockRecordID);
            var g = GameObject.Instantiate(blockPrefab);
            g.transform.position = GetRandomPos();
            g.GetComponent<BlockComponent>().Init(blockTemplate);
        }


    }
}
