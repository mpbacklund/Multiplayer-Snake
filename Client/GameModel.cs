using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Shared.Entities;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using Snake.Storage;
using System.Diagnostics.Metrics;
using Shared.Components;
using Shared;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace Client
{
    public class GameModel
    {
        private ContentManager m_contentManager;
        private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
        private Systems.Network m_systemNetwork = new Systems.Network();
        private Systems.KeyboardInput m_systemKeyboardInput;
        private Systems.Renderer m_systemRenderer;
        private Shared.Systems.Movement m_systemMovement = new Shared.Systems.Movement();
        private Systems.Animation m_systemAnimation = new Systems.Animation();
        private Systems.Particles m_systemParticles = new Systems.Particles();

        private Vector2 m_screenSize;

        private bool loading = false;
        private bool saving = false;

        private int m_highestLeaderboardPosition = -1; 

        private MyRandom m_random = new MyRandom();

        private Entity m_me;

        // SnakeID, headID
        private Dictionary<uint, uint> m_heads = new Dictionary<uint, uint>();

        // headID, score
        private Dictionary<uint, uint> m_topScores = new Dictionary<uint, uint>();

        private Vector2 m_gameSize = Vector2.Zero;

        private SoundEffect m_soundDeath;
        private SoundEffect m_soundEat;

        /// <summary>
        /// This is where everything performs its update.
        /// </summary>
        public void update(TimeSpan elapsedTime)
        {
            // update systems
            m_systemNetwork.update(elapsedTime, MessageQueueClient.instance.getMessages());
            m_systemKeyboardInput.update(elapsedTime);
            m_systemMovement.update(elapsedTime);
            m_systemAnimation.update(elapsedTime);
            m_systemParticles.update(elapsedTime);


            //TODO: this is so broken
            // calculate high scores
            m_topScores.Clear();
            var sortedHeads = m_heads.OrderByDescending(kv => (m_entities[kv.Value].get<SnakeData>().score));

            // Take the top 5 entries and put them into m_topScores
            foreach (var entry in sortedHeads.Take(5))
            {
                m_topScores.Add(entry.Key, entry.Value);
            }

            // Check if the current position is higher than the previous highest position
            int currentLeaderboardPosition = 0;
            foreach (var (index, entry) in sortedHeads.Select((value, index) => (index, value)))
            {
                if (m_me != null)
                {
                    if (entry.Key == m_me.get<SnakeId>().id)
                    {
                        currentLeaderboardPosition = index + 1; // Add 1 to convert from 0-based index to 1-based position
                        break;
                    }
                }

            }

            // Save the current position if it's higher than the previous highest position
            if (currentLeaderboardPosition > m_highestLeaderboardPosition || m_highestLeaderboardPosition < 1)
            {
                m_highestLeaderboardPosition = currentLeaderboardPosition;
            }
        }

        public void render(TimeSpan elapsedTime)
        {
            // don't start rendering until we have received the message from the server specifying the size of the game
            if (m_gameSize != Vector2.Zero && m_me != null)
            {
                m_systemRenderer.update(elapsedTime, m_screenSize, m_gameSize, m_me, m_heads, m_topScores, m_highestLeaderboardPosition, m_systemParticles);
            }
        }

        /// <summary>
        /// This is where all game model initialization occurs.  In the case
        /// of this "game', start by initializing the systems and then
        /// loading the art assets.
        /// </summary>
        public bool initialize(ContentManager contentManager, Vector2 screenSize, SpriteBatch spriteBatch)
        {
            m_soundDeath = contentManager.Load<SoundEffect>("Sounds/death");
            m_soundEat = contentManager.Load<SoundEffect>("Sounds/eat");

            m_screenSize = screenSize;

            m_systemRenderer = new Systems.Renderer(spriteBatch);

            m_contentManager = contentManager;
            m_systemRenderer.loadContent(m_contentManager);
            m_systemParticles.loadContent(m_contentManager);

            m_systemNetwork.registerHandler(Shared.Messages.Type.NewEntity, (TimeSpan elapsedTime, Message message) =>
            {
                handleNewEntity((NewEntity)message);
            });

            m_systemNetwork.registerHandler(Shared.Messages.Type.RemoveEntity, (TimeSpan elapsedTime, Message message) =>
            {
                handleRemoveEntity((RemoveEntity)message);
            });

            m_systemNetwork.registerHandler(Shared.Messages.Type.GameSize, (TimeSpan elapsedTime, Message message) =>
            {
                handleGameSize((GameSize)message);
            });

            m_systemNetwork.registerHandler(Shared.Messages.Type.Turn, (TimeSpan elapsedTime, Message message) =>
            {
                handleTurn((Turn)message);
            });

            m_systemNetwork.registerHandler(Shared.Messages.Type.Collision, (TimeSpan elapsedTime, Message message) =>
            {
                handleCollision((Shared.Messages.Collision)message);
            });

            // get the saved controls from storage
            Dictionary<Shared.Components.Input.Type, Keys> typeToKey = new Dictionary<Shared.Components.Input.Type, Keys>();
            loadKeys(typeToKey);


            m_systemKeyboardInput = new Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
            {
                Tuple.Create(Shared.Components.Input.Type.Up, typeToKey[Shared.Components.Input.Type.Up]),
                Tuple.Create(Shared.Components.Input.Type.Down, typeToKey[Shared.Components.Input.Type.Down]),
                Tuple.Create(Shared.Components.Input.Type.Left, typeToKey[Shared.Components.Input.Type.Left]),
                Tuple.Create(Shared.Components.Input.Type.Right, typeToKey[Shared.Components.Input.Type.Right])
            });

            return true;
        }
        public void shutdown()
        {

        }

        public void signalKeyPressed(Keys key)
        {
            m_systemKeyboardInput.keyPressed(key);
        }

        public void signalKeyReleased(Keys key)
        {
            m_systemKeyboardInput.keyReleased(key);
        }

        /// <summary>
        /// Based upon an Entity received from the server, create the
        /// entity at the client.
        /// </summary>
        private Entity createEntity(Shared.Messages.NewEntity message)
        {
            Entity entity = new Entity(message.id);

            if (message.hasName)
            {
                entity.add(new Shared.Components.Name(message.name));
            }

            if (message.hasAppearance)
            {
                Texture2D texture = m_contentManager.Load<Texture2D>(message.texture);
                entity.add(new Components.Sprite(texture));
            }

            if (message.hasPosition)
            {
                entity.add(new Shared.Components.Position(message.position, message.orientation));
            }

            if (message.hasSize)
            {
                entity.add(new Shared.Components.Size(message.size));
            }

            if (message.hasMovement)
            {
                entity.add(new Shared.Components.Movement(message.moveRate));
            }

            if (message.hasInput)
            {
                m_me = entity;
                entity.add(new Shared.Components.Input(message.inputs));
            }

            if (message.hasSnakeData)
            {
                m_heads.Add(message.snakeId, message.id);
                entity.add(new Shared.Components.SnakeData());
            }

            if (message.hasSnakeId)
            {
                entity.add(new Shared.Components.SnakeId(message.snakeId));

                // If this is a new snake segment, make the tail of its head this new segment
                // we only want to do this for new segments, not ones that are being loaded in,
                // thus the m_me != null to stop this from happening until everything is loaded in
                if (!message.hasSnakeData && m_me != null)
                {
                    m_entities[m_heads[message.snakeId]].get<Shared.Components.SnakeData>().tailId = (int)message.id;
                }
            }

            if (message.hasFood)
            {
                entity.add(new Shared.Components.Food());

                // if the message contains food, we need to add an animation to it
                String texture = message.texture;

                uint subImageIndex;
                uint[] spriteTime;

                switch(texture)
                {
                    case "Textures/snake_roll":
                        subImageIndex = (uint)m_random.nextRange(0, 9.99f);
                        spriteTime = Enumerable.Repeat((uint)100, 10).ToArray();
                        break;
                    case "Textures/food_tongue_out":
                        subImageIndex = (uint)m_random.nextRange(0, 9.99f);
                        spriteTime = Enumerable.Repeat((uint)100, 10).ToArray();
                        break;
                    default:
                        subImageIndex = (uint)m_random.nextRange(0, 2.99f);
                        spriteTime = Enumerable.Repeat((uint)200, 3).ToArray();
                        break;
                }

                entity.add(new Components.Animation(subImageIndex, spriteTime));
            }

            if (message.hasCollision)
            {
                entity.add(new Shared.Components.Collision());
            }

            if (message.hasTurnQueue)
            {
                entity.add(new Shared.Components.TurnQueue(message.queue));
            }
            
            // TODO: new components here

            return entity;
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
            m_systemKeyboardInput.add(entity);
            m_systemRenderer.add(entity);
            m_systemNetwork.add(entity);
            m_systemMovement.add(entity);
            m_systemAnimation.add(entity);
        }

        /// <summary>
        /// All entity lists for the systems must be given a chance to remove
        /// the entity.
        /// </summary>
        private void removeEntity(uint id)
        {
            // if the entity is a head, remove it from the client's list of heads
            // also create particle effects depending on type of entity it was
            if (m_heads.ContainsValue(id))
            {
                foreach (var member in m_heads)
                {
                    if (member.Value == id)
                    {
                        m_systemParticles.playerDeath(m_entities[id].get<Position>().position);

                        m_heads.Remove(member.Key);
                        break;
                    }
                }
            }
            else
            {
                m_systemParticles.foodEaten(m_entities[id].get<Position>().position);
            }

            m_entities.Remove(id);

            m_systemKeyboardInput.remove(id);
            m_systemNetwork.remove(id);
            m_systemRenderer.remove(id);
            m_systemMovement.remove(id);
            m_systemAnimation.remove(id);
        }

        private void handleNewEntity(Shared.Messages.NewEntity message)
        {
            Entity entity = createEntity(message);
            addEntity(entity);
        }

        /// <summary>
        /// Handler for the RemoveEntity message.  It removes the entity from
        /// the client game model (that's us!).
        /// </summary>
        private void handleRemoveEntity(Shared.Messages.RemoveEntity message)
        {
            removeEntity(message.id);
        }

        // gets game size from gamesize message
        private void handleGameSize(Shared.Messages.GameSize message)
        {
            m_gameSize = new Vector2(message.gameWidth, message.gameHeight);
        }

        private void handleTurn(Shared.Messages.Turn message)
        {
            foreach (var entity in m_entities)
            {
                if (entity.Value.contains<SnakeId>() && entity.Value.contains<TurnQueue>())
                {
                    uint snakeId = entity.Value.get<SnakeId>().id;
                    
                    if (snakeId == message.snakeId)
                    {
                        entity.Value.get<TurnQueue>().queue.Enqueue(new Tuple<float, Vector2>(message.direction, message.position));
                    }
                }
            }
        }

        private void handleCollision(Shared.Messages.Collision message)
        {
            m_entities[message.id].get<SnakeData>().kills = message.kills;
            m_entities[message.id].get<SnakeData>().score = message.score;
            m_entities[message.id].get<SnakeData>().state = message.state;

            if (m_me.id == message.id)
            {
                if (m_me.get<Shared.Components.SnakeData>().state != SnakeData.snakeState.dead)
                {
                    m_soundEat.Play();
                }
                else
                {
                    saveScore();
                    m_soundDeath.Play();
                }
            }
        }

        #region Load Controls
        private void loadKeys(Dictionary<Shared.Components.Input.Type, Keys> typeToKey)
        {
            lock (this)
            {
                if (!this.loading)
                {
                    this.loading = true;
                    // Yes, I know the result is not being saved, I dont' need it
                    var result = finalizeLoadKeysAsync(typeToKey);
                    result.Wait();

                }
            }
        }

        private async Task finalizeLoadKeysAsync(Dictionary<Shared.Components.Input.Type, Keys> typeToKey)
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        if (storage.FileExists("Keys.json"))
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("Keys.json", FileMode.Open))
                            {
                                if (fs != null)
                                {
                                    DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Snake.Storage.KeyData));

                                    // get all of the keys for the game
                                    using (StreamReader reader = new StreamReader(fs))
                                    {
                                        // Read lines from the file until the end
                                        string line;
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(line));
                                            Snake.Storage.KeyData keyData = ((Snake.Storage.KeyData)mySerializer.ReadObject(ms));
                                            typeToKey.Add(keyData.Direction, keyData.Key);
                                        }
                                    }
                                    // get the 5 highest scores and return them in HighScores
                                }
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                    }
                }

                this.loading = false;
            });
        }
        #endregion

        #region Save Score
        private void saveScore()
        {
            lock (this)
            {
                if (!this.saving)
                {
                    this.saving = true;

                    // Yes, I know the result is not being saved, I don't need it
                    finalizeSaveAsync();
                }
            }
        }

        private async Task finalizeSaveAsync()
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        // 0. make a list of scoreData and add our new score to it
                        List<ScoreData> scores = new List<ScoreData>();
                        scores.Add(new ScoreData(m_me.get<SnakeData>().score, m_me.get<Name>().name));

                        // 1. load the scores from the file if there are any
                        if (storage.FileExists("HighScores.json"))
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Open))
                            {
                                if (fs != null)
                                {
                                    DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Snake.Storage.ScoreData));

                                    // get all of the high scores saved
                                    using (StreamReader reader = new StreamReader(fs))
                                    {
                                        // Read lines from the file until the end
                                        string line;
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(line));
                                            ScoreData scoreData = (ScoreData)mySerializer.ReadObject(ms);
                                            scores.Add(scoreData);
                                        }
                                    }
                                }
                            }
                        }

                        // 2. get the top 5 (or less) scores

                        var sortedScores = scores.OrderByDescending(s => s.Score).ToList();
                        var top5Scores = sortedScores.Take(5);

                        // 3. clear the file
                        using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Create))
                        {

                        }

                        // 4. add the new top 5 scores to the file
                        foreach (var member in top5Scores)
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Append))
                            {
                                if (fs != null)
                                {
                                    DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(ScoreData));

                                    ScoreData scoreData = new ScoreData(member.Score, member.Name);

                                    mySerializer.WriteObject(fs, scoreData);
                                    using (StreamWriter writer = new StreamWriter(fs))
                                    {
                                        // Write a string with a newline character
                                        writer.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // Ideally show something to the user, but this is demo code :)
                    }
                }

                this.saving = false;
            });
        }
        #endregion
    }
}
