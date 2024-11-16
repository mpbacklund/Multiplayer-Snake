using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Components;

namespace Shared.Messages
{
    public class GameSize : Message
    {
        public uint gameWidth;
        public uint gameHeight;

        public GameSize() : base(Type.GameSize)
        {
        }

        public GameSize(uint gameWidth, uint gameHeight) : base(Type.GameSize)
        {
            this.gameWidth = gameWidth;
            this.gameHeight = gameHeight;
        }

        /// <summary>
        /// </summary>
        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(gameWidth));
            data.AddRange(BitConverter.GetBytes(gameHeight));

            return data.ToArray();
        }

        /// <summary>
        /// </summary>
        public override int parse(byte[] data)
        {
            int offset = base.parse(data);
            this.gameWidth = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);
            this.gameHeight = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

            return offset;
        }
    }
}
