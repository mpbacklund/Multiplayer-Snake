using Client.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Client
{
    public class ClientMain : Game
    {
        private GraphicsDeviceManager m_graphics;
        // private GameModel m_gameModel = new GameModel();

        private GameStateEnum m_nextStateEnum = GameStateEnum.MainMenu;
        private IGameState m_currentState;
        private Dictionary<GameStateEnum, IGameState> m_states;

        public ClientMain()
        {
            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            m_graphics.PreferredBackBufferWidth = 1080;
            m_graphics.PreferredBackBufferHeight = 720;
            m_graphics.ApplyChanges();

            //m_gameModel.initialize(this.Content);

            //create all the game states
            m_states = new Dictionary<GameStateEnum, IGameState>();
            m_states.Add(GameStateEnum.MainMenu, new MainMenuView());
            m_states.Add(GameStateEnum.GamePlay, new GamePlayView());
            m_states.Add(GameStateEnum.HighScores, new HighScoresView());
            m_states.Add(GameStateEnum.Controls, new ControlsView());
            m_states.Add(GameStateEnum.About, new AboutView());
            m_states.Add(GameStateEnum.ChooseName, new ChooseNameView());
            m_states.Add(GameStateEnum.GameInstructions, new GameInstructionsView());

            // start at main menu
            m_currentState = m_states[GameStateEnum.MainMenu];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            foreach (var item in m_states)
            {
                item.Value.initialize(this.GraphicsDevice, m_graphics);
                item.Value.loadContent(this.Content);
            }
        }

        private HashSet<Keys> m_previouslyDown = new HashSet<Keys>();
        protected override void Update(GameTime gameTime)
        {
            m_nextStateEnum = m_currentState.processInput(gameTime);

            // Special case for exiting the game
            if (m_nextStateEnum == GameStateEnum.Exit)
            {
                Exit();
            }

            m_currentState.update(gameTime);

            base.Update(gameTime);

            /*
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
                MessageQueueClient.shutdown();
                Exit();
            }
            */
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            m_currentState.render(gameTime);

            if (m_currentState != m_states[m_nextStateEnum])
            {
                m_currentState = m_states[m_nextStateEnum];
                m_currentState.initializeSession();
            }

            base.Draw(gameTime);
        }
    }
}
