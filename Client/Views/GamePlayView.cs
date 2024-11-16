using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Views
{
    public class GamePlayView : GameStateView
    {
        ContentManager m_content;

        private GameModel m_gameModel;

        private HashSet<Keys> m_previouslyDown;

        public override void initializeSession()
        {
            m_previouslyDown = new HashSet<Keys>();
            m_gameModel = new GameModel();
            MessageQueueClient.shutdown();
            m_gameModel.initialize(m_content, new Vector2(m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), m_spriteBatch);
            
            MessageQueueClient.instance.initialize("localhost", 3000);
        }

        public override void loadContent(ContentManager content)
        {
            m_content = content;
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
                // MessageQueueClient.shutdown();

                return GameStateEnum.MainMenu;
            }

            foreach (var key in m_previouslyDown)
            {
                if (Keyboard.GetState().IsKeyUp(key))
                {
                    m_gameModel.signalKeyReleased(key);
                    m_previouslyDown.Remove(key);
                }
            }

            foreach (var key in Keyboard.GetState().GetPressedKeys())
            {
                if (!m_previouslyDown.Contains(key))
                {
                    m_gameModel.signalKeyPressed(key);
                    m_previouslyDown.Add(key);
                }
            }

            return GameStateEnum.GamePlay;
        }

        public override void update(GameTime gameTime)
        {
            m_gameModel.update(gameTime.ElapsedGameTime);
        }

        public override void render(GameTime gameTime)
        {
            m_gameModel.render(gameTime.ElapsedGameTime);
        }
    }
}
