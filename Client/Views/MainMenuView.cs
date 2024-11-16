using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Views
{
    public class MainMenuView : GameStateView
    {
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private Texture2D m_texBackground;
        private Texture2D m_texPanel;

        private Rectangle m_rectBackground;
        private Rectangle m_rectPanel;

        private enum MenuState
        {
            NewGame,
            HighScores,
            Controls,
            About,
            Quit
        }

        private MenuState m_currentSelection = MenuState.NewGame;
        private bool m_waitForKeyRelease = true;

        public override void loadContent(ContentManager contentManager)
        {
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_texBackground = contentManager.Load<Texture2D>("Textures/mainMenu");
            m_texPanel = contentManager.Load<Texture2D>("Textures/panel");

            m_rectBackground = new Rectangle(
                0,
                0,
                m_graphics.PreferredBackBufferWidth,
                m_graphics.PreferredBackBufferHeight
                );

            m_rectPanel = new Rectangle(
                m_graphics.PreferredBackBufferWidth / 3,
                m_graphics.PreferredBackBufferHeight / 4,
                m_graphics.PreferredBackBufferWidth / 3,
                (int) (m_graphics.PreferredBackBufferHeight / 2.4)
                );
        }
        public override GameStateEnum processInput(GameTime gameTime)
        {
            // This is the technique I'm using to ensure one keypress makes one menu navigation move
            if (!m_waitForKeyRelease)
            {
                // Arrow keys to navigate the menu
                if (Keyboard.GetState().IsKeyDown(Keys.Down) && m_currentSelection != MenuState.Quit)
                {
                    m_currentSelection = m_currentSelection + 1;
                    m_waitForKeyRelease = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Up) && m_currentSelection != MenuState.NewGame)
                {
                    m_currentSelection = m_currentSelection - 1;
                    m_waitForKeyRelease = true;
                }
                
                // If enter is pressed, return the appropriate new state
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.NewGame)
                {
                    return GameStateEnum.ChooseName;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.HighScores)
                {
                    return GameStateEnum.HighScores;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.Controls)
                {
                    return GameStateEnum.Controls;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.About)
                {
                    return GameStateEnum.About;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.Quit)
                {
                    return GameStateEnum.Exit;
                }
            }
            else if (Keyboard.GetState().GetPressedKeyCount() == 0)
            {
                m_waitForKeyRelease = false;
            }

            return GameStateEnum.MainMenu;
        }
        public override void update(GameTime gameTime)
        {
        }
        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.Draw(m_texBackground, m_rectBackground, Color.White);

            m_spriteBatch.Draw(m_texPanel, m_rectPanel, Color.White * 0.6f);

            m_spriteBatch.DrawString(
                m_fontMenuSelect,
                "Snake Eater",
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - m_fontMenuSelect.MeasureString("Snake Eater").X / 2, 75),
                Color.Black
                );

            // I split the first one's parameters on separate lines to help you see them better
            float bottom = drawMenuItem(
                m_currentSelection == MenuState.NewGame ? m_fontMenuSelect : m_fontMenu, 
                "New Game",
                200, 
                m_currentSelection == MenuState.NewGame ? Color.Blue : Color.Black);
            bottom = drawMenuItem(m_currentSelection == MenuState.HighScores ? m_fontMenuSelect : m_fontMenu, "High Scores", bottom, m_currentSelection == MenuState.HighScores ? Color.Blue : Color.Black);
            bottom = drawMenuItem(m_currentSelection == MenuState.Controls ? m_fontMenuSelect : m_fontMenu, "Controls", bottom, m_currentSelection == MenuState.Controls ? Color.Blue : Color.Black);
            bottom = drawMenuItem(m_currentSelection == MenuState.About ? m_fontMenuSelect : m_fontMenu, "About", bottom, m_currentSelection == MenuState.About ? Color.Blue : Color.Black);
            drawMenuItem(m_currentSelection == MenuState.Quit ? m_fontMenuSelect : m_fontMenu, "Quit", bottom, m_currentSelection == MenuState.Quit ? Color.Blue : Color.Black);

            m_spriteBatch.End();
        }

        private float drawMenuItem(SpriteFont font, string text, float y, Color color)
        {
            Vector2 stringSize = font.MeasureString(text);
            m_spriteBatch.DrawString(
                font,
                text,
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y),
                color);

            return y + stringSize.Y;
        }
    }
}