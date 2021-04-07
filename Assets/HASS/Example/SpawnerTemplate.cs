using System.Collections.Generic;
using HASS.Database;
using HASS.Database.Templates;

namespace HASS.Example
{
    /// <summary>
    /// Example spawner template
    /// </summary>
    public class SpawnerTemplate : Template
    {
        public List<Record<BlockCollection>> blockTypes;
        public float spawnTime;
        //public Record<BlockCollection, JumpingBlockTemplate> jump;
    }
}
