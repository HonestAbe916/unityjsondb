### **Scriptable Object Database**

A lightweight JSON Database for storing Scriptable Objects.

## Example

I have blocks of different colors with different characteristics some are static some can move. I also have spawners that can spawn blocks of different types.

#### Template - A Database Record and Scriptable Object

The Template class extends ScriptableObject in order for the Unity Editor to know how to generate the Form. The ScriptableObject's will be not be saved in your project but get converted into one clean JSON file the Database.

    `
    // Define the Templates (Scriptable Objects)
    public class BlockTemplate : Template
    {
        public Color color;
    }

    class MovingBlockTemplate : BlockTemplate
    {
        public float speed;
    }

    public class SpawnerTemplate : Template
    {
        // this will render a dropdown in the Editor of BlockTemplate records
        public List<Record<BlockCollection>> blockTypes;
        public float spawnTime;
    }
    `
#### DatabaseCollection - A collection of Template's

Now we need to create a Database Collections for the BlockTemplates and SpawnerTemplates.


    `
    // Define the Database Collections. This is a DatabaseCollection that stores Templates of Type or subclass of Type BlockTemplate.
    public class BlockCollection : DatabaseCollection<BlockTemplate>
    {
        // customize appearance of collection / add gameObject preview
    }

    // This Database Collection stores Spawner Templates
    public class SpawnerCollection : DatabaseCollection<SpawnerTemplate>
    {
    }

    `

![Example1](https://i.imgur.com/3FlTMCz.png)
![Example2](https://i.imgur.com/qo1GFUf.png)
![Example3](https://i.imgur.com/6g34hE0.png)

You can also use the Record class on a Unity MonoBehavior

    `
    public class SpawnerComponent : MonoBehaviour
    {
        public Record<SpawnerCollection> spawner;

        private SpawnerTemplate m_Template;
        private float m_Timer;

        private void Start()
        {
            m_Template = (SpawnerTemplate)DatabaseComponent.DB.GetTemplate(spawner);
            m_Timer = m_Template.spawnTime;
        }
    }
    `
![ExampleMonoBehavior](https://i.imgur.com/OKvz8so.png)

The result: A mod friendly, human readable, JSON file.

![JSONResult](https://i.imgur.com/DRcIXeT.png)

