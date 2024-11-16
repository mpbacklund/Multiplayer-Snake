using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Shared.Components
{
    public class TurnQueue : Component
    {
        public Queue<Tuple<float, Vector2>> queue;

        public TurnQueue()
        {
            queue = new Queue<Tuple<float, Vector2>>();
        }

        public TurnQueue(Queue<Tuple<float, Vector2>> queue)
        {
            this.queue = new Queue<Tuple<float, Vector2>>(queue);
        }
    }
}
