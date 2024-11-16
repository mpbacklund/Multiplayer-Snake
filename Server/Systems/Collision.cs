using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;

namespace Server.Systems
{
    public class Collision : Shared.Systems.System
    {
        private Action<uint> m_growSnake;
        private Action<uint> m_foodConsumed;
        private Action<uint> m_crashed;
        private Action<uint> m_notifyOfCollision;
        public Collision(Action<uint> foodConsumed, Action<uint> crashed, Action<uint> growSnake, Action<uint> notifyOfCollision)
            : base(
                  typeof(Shared.Components.Position)
                  )
        {
            m_foodConsumed = foodConsumed;
            m_crashed = crashed;
            m_growSnake = growSnake;
            m_notifyOfCollision = notifyOfCollision;
        }

        public override void update(TimeSpan elapsedTime)
        {
        }
        
        public void update(TimeSpan elapsedTime, Vector2 gameSize)
        {
            List<Entity> heads = findHeads();

            // test every head against every other gameObject

            // create a copy of m_entities to prevent errors as we remove things
            var entitiesCopy = new List<Entity>(m_entities.Values);

            foreach (Entity head in heads)
            {
                foreach (Entity entity in entitiesCopy)
                {
                    // don't check entity against itself for collision
                    if (!(entity.contains<SnakeId>() && head.get<SnakeId>().id == entity.get<SnakeId>().id) && collides(head, entity))
                    {
                        if (entity.contains<Shared.Components.Food>())
                        {
                            head.get<Shared.Components.SnakeData>().score++;
                            var foodEaten = head.get<Shared.Components.SnakeData>().foodToNextSegment++;
                            m_foodConsumed(entity.id);

                            if (foodEaten >= 4) 
                            {
                                head.get<Shared.Components.SnakeData>().foodToNextSegment = 0;

                                m_growSnake(head.id);
                            }
                            m_notifyOfCollision(head.id);
                        }

                        // here we know that we crashed into something, so we check both entities to see if either is invincible before killing one
                        else if(head.get<SnakeData>().state != SnakeData.snakeState.invincible)
                        {
                            uint killerId = entity.get<Shared.Components.SnakeId>().id;

                            // increase kills on client and server for whatever killed us
                            // as long as it is not another head
                            if (!entity.contains<Shared.Components.SnakeData>())
                            {
                                uint killerHead = increaseKills(killerId);
                                m_notifyOfCollision(killerHead);
                            }

                            // set our state to dead, then let client and server know
                            head.get<SnakeData>().state = SnakeData.snakeState.dead;
                            m_notifyOfCollision(head.id);
                            m_crashed(head.id);
                        }
                    }
                }

                // test if the entity hit a wall
                Vector2 position = head.get<Position>().position;
                Vector2 size = head.get<Size>().size;
                if (position.X - (size.X / 2) < 0 || position.X + (size.X / 2) > gameSize.X || position.Y - (size.Y / 2) < 0 || position.Y + (size.Y / 2) > gameSize.Y)
                {
                    head.get<SnakeData>().state = SnakeData.snakeState.dead;
                    m_notifyOfCollision(head.id);
                    m_crashed(head.id);
                }
            }
        }

        private List<Entity> findHeads()
        {
            List<Entity> heads = new List<Entity>();
            foreach (var entity in m_entities.Values)
            {
                if (entity.contains<Shared.Components.SnakeData>())
                {
                    heads.Add(entity);
                }
            }
            return heads;
        }

        // increase kills for the snake with the given id
        private uint increaseKills(uint snakeId)
        {
            foreach (Entity entity in m_entities.Values)
            {
                if (entity.contains<Shared.Components.SnakeId>() && entity.get<Shared.Components.SnakeId>().id == snakeId && entity.contains<SnakeData>())
                {
                    entity.get<SnakeData>().kills++;
                    entity.get<SnakeData>().score += 10;
                    return entity.id;
                }
            }
            return 0;
        }

        private bool collides(Entity head, Entity other)
        {
            // Get radius of a and b
            float headRadius = head.get<Size>().size.X / 2;
            float otherRadius = other.get<Size>().size.X / 2;

            Vector2 headPosition = head.get<Position>().position;
            Vector2 otherPosition = other.get<Position>().position;

            float distanceBetween = Vector2.Distance(headPosition, otherPosition);

            if (distanceBetween <= headRadius + otherRadius)
            {
                return true;
            }
            return false;
        }
    }
}