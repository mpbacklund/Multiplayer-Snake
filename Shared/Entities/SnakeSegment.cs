using Microsoft.Xna.Framework;
using Shared.Components;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Shared.Messages;

namespace Shared.Entities
{
    public class SnakeSegment
    {
        public static Entity create(uint snakeId, string texture, Vector2 position, float size, float moveRate, float orientation)
        {
            Entity entity = new Entity();

            entity.add(new Appearance(texture));
            entity.add(new SnakeId(snakeId));

            entity.add(new Position(position, orientation));
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Movement(moveRate));
            entity.add(new TurnQueue());
            entity.add(new Shared.Components.Collision());

            return entity;
        }
    }
}