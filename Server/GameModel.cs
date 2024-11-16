using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using System.Linq;
using System;

namespace Server
{
    public class GameModel
    {
        private uint m_nextSnakeId = 0;
        private HashSet<int> m_clients = new HashSet<int>();
        private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
        private Dictionary<int, uint> m_clientToEntityId = new Dictionary<int, uint>();
        private Dictionary<uint, int> m_snaketoclientId = new Dictionary<uint, int>();

        Systems.Network m_systemNetwork = new Server.Systems.Network();

        // given any snake id, returns the client to which that snake belongs
        Shared.Systems.Movement m_systemMovement = new Shared.Systems.Movement();

        Systems.Collision m_systemCollision;

        private Shared.MyRandom m_random;

        private bool m_connected = false;

        public uint m_gameWidth = 3000;
        public uint m_gameHeight = 2000;

        private double m_timeSinceLastBroadcast = 0;

        private int m_foodCurrentlyInGame = 0;
        private double m_timeToAddFood = 0;

        /// <summary>
        /// This is where the server-side simulation takes place.  Messages
        /// from the network are processed and then any necessary client
        /// updates are sent out.
        /// </summary>
        public void update(TimeSpan elapsedTime)
        {
            m_systemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
            m_systemMovement.update(elapsedTime);
            m_systemCollision.update(elapsedTime, new Vector2(m_gameWidth, m_gameHeight));

            // periodically send an update on all game elements to clients to keep everything synced

            m_timeSinceLastBroadcast += elapsedTime.TotalMilliseconds;

            if (m_timeSinceLastBroadcast > 1000)
            {
                foreach (var entity in m_entities.Values)
                {
                    if (entity.contains<Movement>()) 
                    {
                        var message = new Shared.Messages.UpdateEntity(entity, elapsedTime);
                        MessageQueueServer.instance.broadcastMessage(message);
                    } 
                }
                m_timeSinceLastBroadcast = 0;
            }

            // create 5 food every 5 seconds while total food is under 300
            m_timeToAddFood += elapsedTime.TotalMilliseconds;
            if (m_timeToAddFood > 5000 && m_foodCurrentlyInGame < 300)
            {
                createFood(5);
                m_timeToAddFood = 0;
            }
            
        }

        /// <summary>
        /// Setup notifications for when new clients connect.
        /// </summary>
        public bool initialize()
        {
            m_systemNetwork.registerHandler(Shared.Messages.Type.Join, handleJoin);
            m_systemNetwork.registerDisconnectHandler(handleDisconnect);

            MessageQueueServer.instance.registerConnectHandler(handleConnect);

            Action<uint> foodConsumed = (id) =>
            {
                // remove food from client and server
                removeEntity(id);

                Message message = new Shared.Messages.RemoveEntity(id);
                MessageQueueServer.instance.broadcastMessage(message);

                m_foodCurrentlyInGame--;
            };

            Action<uint> crashed = (id) =>
            {
                uint snakeId = m_entities[id].get<SnakeId>().id;

                // send game over message to client who crashed

                // Create a copy of the values to iterate over
                var entities = new List<Entity>(m_entities.Values);

                foreach (var entity in entities)
                {
                    if (entity.contains<SnakeId>() && entity.get<SnakeId>().id == snakeId)
                    {
                        Message message = new Shared.Messages.RemoveEntity(entity.id);
                        MessageQueueServer.instance.broadcastMessage(message);

                        createFood(entity.get<Shared.Components.Position>().position, entity.get<Shared.Components.Size>().size.X);
                        removeEntity(entity.id);
                    }
                }
            };

            Action<uint> growSnake = (id) =>
            {
                addSegment(id);
            };

            Action<uint> notifyOfCollision = (id) =>
            {
                // tell clients that a snake ate food or had a kill
                Message message = new Shared.Messages.Collision(m_entities[id]);
                MessageQueueServer.instance.broadcastMessage(message);
            };

            m_systemCollision = new Systems.Collision(foodConsumed, crashed, growSnake, notifyOfCollision);

            m_random = new Shared.MyRandom();

            createFood(200);

            return true;
        }

        /// <summary>
        /// Give everything a chance to gracefully shutdown.
        /// </summary>
        public void shutdown()
        {

        }

        /// <summary>
        /// Upon connection of a new client, create a player entity and
        /// send that info back to the client, along with adding it to
        /// the server simulation.
        /// </summary>
        private void handleConnect(int clientId)
        {
            m_clients.Add(clientId);

            MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.ConnectAck());
        }

        /// <summary>
        /// When a client disconnects, need to tell all the other clients
        /// of the disconnect.
        /// </summary>
        /// <param name="clientId"></param>
        private void handleDisconnect(int clientId)
        {
            // TODO: remove entire snake

            m_clients.Remove(clientId);

            if (m_clientToEntityId.ContainsKey(clientId) && m_entities.ContainsKey(m_clientToEntityId[clientId]))
            {
                uint snakeId = m_entities[m_clientToEntityId[clientId]].get<SnakeId>().id;

                foreach (var entity in m_entities.Values)
                {
                    if (entity.contains<SnakeId>() && entity.get<SnakeId>().id == snakeId)
                    {
                        Message message = new Shared.Messages.RemoveEntity(entity.id);
                        MessageQueueServer.instance.broadcastMessage(message);

                        removeEntity(entity.id);
                    }
                }

                removeEntity(m_clientToEntityId[clientId]);

                m_snaketoclientId.Remove(snakeId);
                m_clientToEntityId.Remove(clientId);
            }
            else
            {
                // Handle the case when the clientId is not found in m_clientToEntityId
                // For example, log an error or perform other cleanup tasks
            }
        }

        /// <summary>
        /// As entities are added to the game model, they are run by the systems
        /// to see if they are interested in knowing about them during their
        /// updates.
        /// </summary>
        private void addEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            m_entities[entity.id] = entity;
            m_systemNetwork.add(entity);
            m_systemMovement.add(entity);
            m_systemCollision.add(entity);
        }

        /// <summary>
        /// All entity lists for the systems must be given a chance to remove
        /// the entity.
        /// </summary>
        private void removeEntity(uint id)
        {
            m_entities.Remove(id);
            m_systemNetwork.remove(id);
            m_systemMovement.remove(id);
            m_systemCollision.remove(id);
        }

        private Entity addSegment(uint id)
        {
            var SnakeData = m_entities[id].get<SnakeData>();

            var tailId = id;
            if (SnakeData.tailId != 0)
            {
                tailId = (uint) SnakeData.tailId;
            }

            var tail = m_entities[tailId];

            Vector2 tailPosition = tail.get<Position>().position;
            float tailDirection = tail.get<Position>().orientation;
            Vector2 distance = 2 * tail.get<Size>().size / 3;

            float x = tailPosition.X - distance.X * (float)Math.Cos(tailDirection);
            float y = tailPosition.Y - distance.Y * (float)Math.Sin(tailDirection);

            Entity newTail = Shared.Entities.SnakeSegment.create(
                tail.get<SnakeId>().id,
                "Textures/body",
                new Vector2(x, y),
                tail.get<Size>().size.X,
                tail.get<Movement>().moveRate,
                tail.get<Position>().orientation
            );
            
            // copy turnqueue to new segment
            if (tail.contains<TurnQueue>())
            {
                newTail.get<TurnQueue>().queue = new Queue<Tuple<float, Vector2>>(tail.get<TurnQueue>().queue);
            }

            addEntity(newTail);

            m_entities[id].get<SnakeData>().tailId = (int) newTail.id;

            // Send the new segment to the client of the snake it belongs to

            int clientId = m_snaketoclientId[newTail.get<SnakeId>().id];
            MessageQueueServer.instance.sendMessage(clientId, new NewEntity(newTail));

            // Step 4: Let all other clients know about this new segment

            // We change the appearance for the entity for all other clients to a different texture
            newTail.remove<Appearance>();
            newTail.add(new Appearance("Textures/body_enemy"));

            Message messageNewEntity = new NewEntity(newTail);
            foreach (int otherId in m_clients)
            {
                if (otherId != clientId)
                {
                    MessageQueueServer.instance.sendMessage(otherId, messageNewEntity);
                }
            }

            return newTail;
            // TODO: report?
        }

        private void createFood(uint number)
        {
            for (int i = 0; i < number; i++)
            {
                float x = m_random.nextRange(30, m_gameWidth - 30);
                float y = m_random.nextRange(30, m_gameHeight - 30);

                float size = m_random.nextRange(40, 50);
                int type = (int) m_random.nextRange(0, 2.99f); // returns 0, 1, or 2

                String texture;
                switch (type)
                {
                    case 0:
                        texture = "Textures/snake_roll";
                        break;
                    case 1:
                        texture = "Textures/food_tongue_out";
                        break;
                    default: // case 2
                        texture = "Textures/food_rock";
                        break;
                }

                var food = Shared.Entities.Food.create(texture, new Vector2(x, y), size);
                addEntity(food);

                Message message = new Shared.Messages.NewEntity(food);
                MessageQueueServer.instance.broadcastMessage(message);

                m_foodCurrentlyInGame++;
            }
        }

        private void createFood(Vector2 position, float size)
        {
            int type = (int)m_random.nextRange(0, 2.99f); // returns 0, 1, or 2

            String texture;
            switch (type)
            {
                case 0:
                    texture = "Textures/snake_roll";
                    break;
                case 1:
                    texture = "Textures/food_tongue_out";
                    break;
                default: // case 2
                    texture = "Textures/food_rock";
                    break;
            }

            var food = Shared.Entities.Food.create(texture, position, size);
            addEntity(food);

            Message message = new Shared.Messages.NewEntity(food);
            MessageQueueServer.instance.broadcastMessage(message);

            m_foodCurrentlyInGame++;
        }

        /// <summary>
        /// For the indicated client, sends messages for all other entities
        /// currently in the game simulation.
        /// </summary>
        private void reportAllEntities(int clientId)
        {
            foreach (var item in m_entities)
            {
                MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.NewEntity(item.Value));
            }
        }

        private void reportGameSize(int clientId)
        {
            MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.GameSize(m_gameWidth, m_gameHeight));
        }

        /// <summary>
        /// Handler for the Join message.  It gets a player entity created,
        /// added to the server game model, and notifies the requesting client
        /// of the player.
        /// </summary>
        private void handleJoin(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message)
        {
            // get name from join message
            Shared.Messages.Join messageJoin = (Shared.Messages.Join) message;
            Console.WriteLine($"{messageJoin.name} joined the game!");

            // Step 0: tell the newly joined player the size of the game
            reportGameSize(clientId);

            // Step 1: Tell the newly connected player about all other entities
            reportAllEntities(clientId);

            // Step 2: Create an entity for the newly joined player and sent it
            //         to the newly joined client

            // generate random starting position for player
            float x = m_random.nextRange(m_gameWidth / 8, m_gameWidth - (m_gameWidth / 8));
            float y = m_random.nextRange(m_gameHeight / 8, m_gameHeight - (m_gameHeight / 8));

            Entity player = Shared.Entities.SnakeHead.create(m_nextSnakeId ++, "Textures/head", messageJoin.name, new Vector2(x, y), 50, 0.3f);
            addEntity(player);
            m_clientToEntityId[clientId] = player.id;
            m_snaketoclientId[player.get<SnakeId>().id] = clientId;

            // Step 3: Send the new player entity to the newly joined client
            MessageQueueServer.instance.sendMessage(clientId, new NewEntity(player));

            // Step 4: Let all other clients know about this new player entity

            // We change the appearance for a player ship entity for all other clients to a different texture
            // TODO: change to a different color
            player.remove<Appearance>();
            player.add(new Appearance("Textures/head_enemy"));

            // Remove components not needed for "other" players
            player.remove<Shared.Components.Input>();

            Message messageNewEntity = new NewEntity(player);
            foreach (int otherId in m_clients)
            {
                if (otherId != clientId)
                {
                    MessageQueueServer.instance.sendMessage(otherId, messageNewEntity);
                }
            }

            for (int i = 0; i< 3; i++)
            {
                var segment = addSegment(player.id);
            }
        }
    }
}
