using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Components;
using Shared.Entities;

namespace Shared.Messages
{
    public class Collision : Shared.Messages.Message
    {
        public int score;
        public int kills;
        public Shared.Components.SnakeData.snakeState state;
        public uint id;

        public Collision() : base(Type.Collision)
        {
        }

        public Collision(Entity entity) : base(Type.Collision)
        {
            var snakeData = entity.get<SnakeData>();

            this.score = snakeData.score;
            this.kills = snakeData.kills;
            this.state = snakeData.state;
            this.id = entity.id;
        }

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());

            data.AddRange(BitConverter.GetBytes(score));
            data.AddRange(BitConverter.GetBytes(kills));
            data.AddRange(BitConverter.GetBytes((int) state));
            data.AddRange(BitConverter.GetBytes(id));

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            this.score = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            this.kills = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            this.state = (Shared.Components.SnakeData.snakeState) BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            this.id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

            return offset;
        }
    }
}
