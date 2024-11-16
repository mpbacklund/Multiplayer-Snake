using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Microsoft.Xna.Framework;

namespace Server.Systems
{
    public class Network : Shared.Systems.System
    {
        public delegate void Handler(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message);
        public delegate void DisconnectHandler(int clientId);
        public delegate void InputHandler(Entity entity, Shared.Components.Input.Type type, TimeSpan elapsedTime);

        private Dictionary<Shared.Messages.Type, Handler> m_commandMap = new Dictionary<Shared.Messages.Type, Handler>();
        private DisconnectHandler m_disconnectHandler;

        private HashSet<uint> m_reportThese = new HashSet<uint>();

        /// <summary>
        /// Primary activity in the constructor is to setup the command map
        /// that maps from message types to their handlers.
        /// </summary>
        public Network() :
            base(
                typeof(Shared.Components.Movement),
                typeof(Shared.Components.Position)
            )
        {
        

            // Register our own disconnect handler
            registerHandler(Shared.Messages.Type.Disconnect, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
            {
                if (m_disconnectHandler != null)
                {
                    m_disconnectHandler(clientId);
                }
            });

            // Register our own input handler
            registerHandler(Shared.Messages.Type.Input, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
            {
                handleInput((Shared.Messages.Input)message);
            });
        }

        // Have to implement this because it is abstract in the base class
        public override void update(TimeSpan elapsedTime) { }

        /// <summary>
        /// Have our own version of update, because we need a list of messages to work with, and
        /// messages aren't entities.
        /// </summary>
        public void update(TimeSpan elapsedTime, Queue<Tuple<int, Message>> messages)
        {
            if (messages != null)
            {
                while (messages.Count > 0)
                {
                    var message = messages.Dequeue();
                    if (m_commandMap.ContainsKey(message.Item2.type))
                    {
                        m_commandMap[message.Item2.type](message.Item1, elapsedTime, message.Item2);
                    }
                }
            }

            // Send updated game state updates back out to connected clients
            updateClients(elapsedTime);
        }

        public void registerDisconnectHandler(DisconnectHandler handler)
        {
            m_disconnectHandler = handler;
        }

        public void registerHandler(Shared.Messages.Type type, Handler handler)
        {
            m_commandMap[type] = handler;
        }

        /// <summary>
        /// Handler for the Input message.  This simply passes the responsibility
        /// to the registered input handler.
        /// </summary>
        private void handleInput(Shared.Messages.Input message)
        {
            var entity = m_entities[message.entityId];
            bool turned = Shared.Entities.Utility.rotate(entity, message.inputs);
            m_reportThese.Add(message.entityId);

            // If we actually turned, send the new turn point out to all connected clients
            if (turned)
            {
                var turn = entity.get<Shared.Components.Position>();
                var snakeId = entity.get<Shared.Components.SnakeId>();
                MessageQueueServer.instance.broadcastMessage(new Turn(turn.orientation, turn.position, snakeId.id));
                updateTurnQueues(snakeId.id, turn.orientation, turn.position);
                // update turnqueue of all server entities with same snakeid
                // MessageQueueServer.instance.broadcastMessage(new NewEntity(turnPoint));
            }

            // TODO: do I update server side entities here?
        }

        private void updateTurnQueues(uint snakeId, float direction, Vector2 position)
        {
            foreach (var entity in m_entities)
            {
                if (entity.Value.contains<SnakeId>() && entity.Value.get<SnakeId>().id == snakeId)
                {
                    if (entity.Value.contains<TurnQueue>())
                    {
                        entity.Value.get<TurnQueue>().queue.Enqueue(new Tuple<float, Vector2>(direction, position));
                    }
                }
            }
        }

        /// <summary>
        /// For the entities that have updates, send those updates to all
        /// connected clients.
        /// </summary>
        private void updateClients(TimeSpan elapsedTime)
        {
            foreach (var entityId in m_reportThese)
            {
                var entity = m_entities[entityId];
                var message = new Shared.Messages.UpdateEntity(entity, elapsedTime);
                MessageQueueServer.instance.broadcastMessageWithLastId(message);
            }

            m_reportThese.Clear();
        }
    }
}
