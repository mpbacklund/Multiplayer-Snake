using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Components;
using Microsoft.Xna.Framework;
using System.Xml.Linq;

namespace Shared.Messages
{
    public class Turn : Message
    {
        public float direction;
        public Vector2 position;
        public uint snakeId;

        public Turn() : base(Type.Turn)
        {

        }

        public Turn(float direction, Vector2 position, uint snakeId) : base(Type.Turn)
        {
            this.direction = direction;
            this.position = position;
            this.snakeId = snakeId;
        }

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());

            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            data.AddRange(BitConverter.GetBytes(direction));
            data.AddRange(BitConverter.GetBytes(snakeId));

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            float positionX = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            float positionY = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            this.position = new Vector2(positionX, positionY);
            this.direction = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            this.snakeId = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

            return offset;
        }
    }
}
