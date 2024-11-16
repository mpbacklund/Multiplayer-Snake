using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Shared.Components;
using Shared.Entities;
using System;
using System.Data.Common;
using System.Text;

namespace Shared.Messages
{
    public class NewEntity : Message
    {
        public NewEntity(Entity entity) : base(Type.NewEntity)
        {
            this.id = entity.id;

            if (entity.contains<SnakeData>())
            {
                this.hasSnakeData = true;
                var data = entity.get<SnakeData>();
                this.score = data.score;
                this.tailId = data.tailId;
                this.foodToNextSegment = data.foodToNextSegment;
                this.kills = data.kills;
                this.state = data.state;
            }

            if (entity.contains<Shared.Components.Collision>())
            {
                this.hasCollision = true;
            }

            if (entity.contains<Shared.Components.Food>())
            {
                this.hasFood = true;
            }

            if (entity.contains<SnakeId>())
            {
                this.hasSnakeId = true;
                this.snakeId = entity.get<SnakeId>().id;
            }

            if (entity.contains<Name>())
            {
                this.hasName = true;
                this.name = entity.get<Name>().name;
            }

            if (entity.contains<Appearance>())
            {
                this.hasAppearance = true;
                this.texture = entity.get<Appearance>().texture;
            }
            else
            {
                this.texture = "";
            }

            if (entity.contains<Position>())
            {
                this.hasPosition = true;
                this.position = entity.get<Position>().position;
                this.orientation = entity.get<Position>().orientation;
            }

            if (entity.contains<Size>())
            {
                this.hasSize = true;
                this.size = entity.get<Size>().size;
            }

            if (entity.contains<Movement>())
            {
                this.hasMovement = true;
                this.moveRate = entity.get<Movement>().moveRate;
            }

            if (entity.contains<Components.Input>())
            {
                this.hasInput = true;
                this.inputs = entity.get<Components.Input>().inputs;
            }
            else
            {
                this.inputs = new List<Components.Input.Type>();
            }

            
            if (entity.contains<Components.TurnQueue>())
            {
                this.hasTurnQueue = true;
                this.queue = entity.get<TurnQueue>().queue;
            }
            
        }
        public NewEntity() : base(Type.NewEntity)
        {
            this.texture = "";
            this.inputs = new List<Components.Input.Type>();
        }

        public uint id { get; private set; }

        // SnakeData
        public bool hasSnakeData { get; private set; } = false;
        public int tailId { get; private set; }
        public int score {  get; private set; }
        public int kills { get; private set; }
        public int foodToNextSegment { get; private set; }
        public Shared.Components.SnakeData.snakeState state { get; private set; }

        // Collision
        public bool hasCollision { get; private set; } = false;

        // Food
        public bool hasFood { get; private set; } = false;

        // SnakeId
        public bool hasSnakeId { get; private set; } = false;
        public uint snakeId { get; private set; }

        // TurnQueue
        public bool hasTurnQueue { get; private set; } = false;
        public Queue<Tuple<float, Vector2>> queue { get; private set; }

        // Name
        public bool hasName { get; private set; } = false;
        public string name { get; private set; }

        // Appearance
        public bool hasAppearance { get; private set; } = false;
        public string texture { get; private set; }

        // Position
        public bool hasPosition { get; private set; } = false;
        public Vector2 position { get; private set; }
        public float orientation { get; private set; }
        
        // size
        public bool hasSize { get; private set; } = false;
        public Vector2 size { get; private set; }

        // Movement
        public bool hasMovement { get; private set; } = false;
        public float moveRate { get; private set; }

        // Input
        public bool hasInput { get; private set; } = false;
        public List<Components.Input.Type> inputs { get; private set; }

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(id));

            data.AddRange(BitConverter.GetBytes(hasSnakeData));
            if (hasSnakeData)
            {
                data.AddRange(BitConverter.GetBytes(tailId));
                data.AddRange(BitConverter.GetBytes(score));
                data.AddRange(BitConverter.GetBytes(foodToNextSegment));
                data.AddRange(BitConverter.GetBytes(kills));
                data.AddRange(BitConverter.GetBytes((int) state));
            }

            data.AddRange(BitConverter.GetBytes(hasFood));

            data.AddRange(BitConverter.GetBytes(hasCollision));

            data.AddRange(BitConverter.GetBytes(hasSnakeId));
            if (hasSnakeId)
            {
                data.AddRange(BitConverter.GetBytes(snakeId));
            }

            data.AddRange(BitConverter.GetBytes(hasName));
            if (hasName)
            {
                data.AddRange(BitConverter.GetBytes(name.Length));
                data.AddRange(Encoding.UTF8.GetBytes(name));
            }

            data.AddRange(BitConverter.GetBytes(hasAppearance));
            if (hasAppearance)
            {
                data.AddRange(BitConverter.GetBytes(texture.Length));
                data.AddRange(Encoding.UTF8.GetBytes(texture));
            }

            data.AddRange(BitConverter.GetBytes(hasPosition));
            if (hasPosition)
            {
                data.AddRange(BitConverter.GetBytes(position.X));
                data.AddRange(BitConverter.GetBytes(position.Y));
                data.AddRange(BitConverter.GetBytes(orientation));
            }

            data.AddRange(BitConverter.GetBytes(hasSize));
            if (hasSize)
            {
                data.AddRange(BitConverter.GetBytes(size.X));
                data.AddRange(BitConverter.GetBytes(size.Y));
            }

            data.AddRange(BitConverter.GetBytes(hasMovement));
            if (hasMovement)
            {
                data.AddRange(BitConverter.GetBytes(moveRate));
            }

            data.AddRange(BitConverter.GetBytes(hasInput));
            if (hasInput)
            {
                data.AddRange(BitConverter.GetBytes(inputs.Count));
                foreach (var input in inputs)
                {
                    data.AddRange(BitConverter.GetBytes((UInt16)input));
                }
            }

            data.AddRange(BitConverter.GetBytes(hasTurnQueue));
            if (hasTurnQueue)
            {
                data.AddRange(BitConverter.GetBytes(queue.Count));
                foreach (var member in queue)
                {
                    data.AddRange(BitConverter.GetBytes(member.Item2.X));
                    data.AddRange(BitConverter.GetBytes(member.Item2.Y));
                    data.AddRange(BitConverter.GetBytes(member.Item1));
                }
            }
            

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            this.id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

            this.hasSnakeData = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasSnakeData)
            {
                this.tailId = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.score = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.foodToNextSegment = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.kills = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.state = (Shared.Components.SnakeData.snakeState) BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
            }

            this.hasFood = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);

            this.hasCollision = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);

            this.hasSnakeId = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasSnakeId)
            {
                this.snakeId = BitConverter.ToUInt32(data, offset);
                offset += sizeof(uint);
            }

            this.hasName = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasName)
            {
                int nameSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.name = Encoding.UTF8.GetString(data, offset, nameSize);
                offset += nameSize;
            }

            this.hasAppearance = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasAppearance)
            {
                int textureSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.texture = Encoding.UTF8.GetString(data, offset, textureSize);
                offset += textureSize;
            }

            this.hasPosition = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasPosition)
            {
                float positionX = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                float positionY = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                this.position = new Vector2(positionX, positionY);
                this.orientation = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            this.hasSize = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasSize)
            {
                float sizeX = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                float sizeY = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                this.size = new Vector2(sizeX, sizeY);
            }

            this.hasMovement = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasMovement)
            {
                this.moveRate = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            this.hasInput = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasInput)
            {
                int howMany = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                for (int i = 0; i < howMany; i++)
                {
                    inputs.Add((Components.Input.Type)BitConverter.ToUInt16(data, offset));
                    offset += sizeof(UInt16);
                }
            }

            this.hasTurnQueue = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasTurnQueue)
            {
                queue = new Queue<Tuple<float, Vector2>>();
                int howMany = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                for (int i = 0; i < howMany; i++)
                {
                    // read X component of Vector2
                    float x = BitConverter.ToSingle(data, offset);
                    offset += sizeof(float);

                    // Read Y component of Vector2
                    float y = BitConverter.ToSingle(data, offset);
                    offset += sizeof(float);

                    // Read the float value
                    float floatValue = BitConverter.ToSingle(data, offset);
                    offset += sizeof(float);

                    // Create a Tuple and add it to the parsedQueue
                    queue.Enqueue(new Tuple<float, Vector2>(floatValue, new Vector2(x, y)));
                }
            }

            return offset;
        }
    }
}
