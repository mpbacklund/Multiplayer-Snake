using Shared.Components;
using Microsoft.Xna.Framework;

namespace Shared.Entities
{
    public class Food
    {
        public static Entity create(string texture, Vector2 position, float size)
        {
            Entity entity = new Entity(); // creates new entity and assigns the entity an id

            entity.add(new Appearance(texture));

            entity.add(new Position(position));
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Shared.Components.Food());
            entity.add(new Collision());

            return entity;
        }
    }
}
