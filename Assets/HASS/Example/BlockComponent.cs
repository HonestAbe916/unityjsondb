using UnityEngine;
namespace HASS.Example
{
    /// <summary>
    /// Example, sets the block material color to the one specified in its Template.
    /// </summary>
    public class BlockComponent : MonoBehaviour
    {
        private BlockTemplate m_Template;
        private bool moving;

        public void Init(BlockTemplate template)
        {
            m_Template = template;
            GetComponent<MeshRenderer>().material.color = template.color;
            moving = m_Template.GetType() == typeof(MovingBlockTemplate);
        }

        private void Update()
        {
            if (moving)
            {
                Vector3 temp = transform.position;
                temp.x += Random.Range(-2f, 2f);
                temp.z += Random.Range(-2f, 2f);
                var template = (MovingBlockTemplate)m_Template;
                transform.position = Vector3.MoveTowards(transform.position, temp, template.speed);
            }
        }
    }
}
