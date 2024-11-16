using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Views
{
    public class AboutView : GameStateView
    {
        private SpriteFont m_font;
        private SpriteFont m_fontMenuSelect;

        private Texture2D m_texBackground;
        private Texture2D m_texPanel;

        private Rectangle m_rectBackground;
        private Rectangle m_rectPanel;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_texBackground = contentManager.Load<Texture2D>("Textures/menu2");
            m_texPanel = contentManager.Load<Texture2D>("Textures/panel");

            m_rectBackground = new Rectangle(
                0,
                0,
                m_graphics.PreferredBackBufferWidth,
                m_graphics.PreferredBackBufferHeight
                );

            m_rectPanel = new Rectangle(
                50,
                50,
                m_graphics.PreferredBackBufferWidth - 100,
                m_graphics.PreferredBackBufferHeight -100
                );
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.About;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.Draw(m_texBackground, m_rectBackground, Color.White);

            m_spriteBatch.Draw(m_texPanel, m_rectPanel, Color.White * 0.6f);

            float bottom = drawMenuItem(
                m_fontMenuSelect,
                "Credits",
                120, // Adjust vertical position to separate from previous item
                Color.Blue); // Choose a color for the message
            bottom = drawMenuItem(
                m_font,
                "Programmer: Matthew Backlund",
                bottom + 20,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Food Particles: davididev",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Background: macrovector on Freepik",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Large Snake Food: AntumDeluge",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Small Snake Food: Calciumtrice",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Sounds: rubberduck",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Background Art: Adobe Firefly",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_font,
                "Everything else: Matthew Backlund",
                bottom,
                Color.Black);


            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
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
