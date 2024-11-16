using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Views
{
    internal class ChooseNameView : GameStateView
    {
        StringBuilder m_name;

        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;

        private Texture2D m_texBackground;

        private Rectangle m_rectBackground;
        private Rectangle m_rectPanel;

        private Vector2 m_instructions_position1;
        private Vector2 m_instructions_position2;
        private string m_instructions_text1 = "Type in your name.";
        private string m_instructions_text2 = "When you are done, press enter";

        private Rectangle m_rectInputBox;
        private Texture2D m_texPanel;

        private bool m_isShiftPressed = false;
        private List<Keys> m_keysPressed = new List<Keys>();

        private float m_timeSinceLastBackspace = 0f;

        public override void initializeSession()
        {
            m_name = new StringBuilder("Player1");

            m_keysPressed.Add(Keys.Enter);
            m_keysPressed.Add(Keys.Escape);
        }

        public override void loadContent(ContentManager content)
        {
            m_fontMenu = content.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = content.Load<SpriteFont>("Fonts/menu-select");
            m_texBackground = content.Load<Texture2D>("Textures/menu2");
            m_texPanel = content.Load<Texture2D>("Textures/panel");

            m_instructions_position1 = new Vector2((m_graphics.PreferredBackBufferWidth / 2) - (m_fontMenu.MeasureString(m_instructions_text1).X / 2), 200);
            m_instructions_position2 = new Vector2((m_graphics.PreferredBackBufferWidth / 2) - (m_fontMenu.MeasureString(m_instructions_text2).X / 2), 200 + (m_fontMenu.MeasureString(m_instructions_text2).Y));

            int inputBoxWidth = 600; // Width of the input box
            int inputBoxHeight = (int)m_fontMenu.MeasureString(m_instructions_text1).Y + 30; // Height of the input box
            int inputBoxX = (m_graphics.PreferredBackBufferWidth - inputBoxWidth) / 2; // X-coordinate
            int inputBoxY = (m_graphics.PreferredBackBufferHeight - inputBoxHeight) / 2; // Y-coordinate

            m_rectInputBox = new Rectangle(inputBoxX, inputBoxY, inputBoxWidth, inputBoxHeight);

            m_rectBackground = new Rectangle(
                0,
                0,
                m_graphics.PreferredBackBufferWidth,
                m_graphics.PreferredBackBufferHeight
                );

            m_rectPanel = new Rectangle(
                m_graphics.PreferredBackBufferWidth / 8,
                m_graphics.PreferredBackBufferHeight / 4,
                (int) (m_graphics.PreferredBackBufferWidth * 0.75),
                (int) (m_graphics.PreferredBackBufferHeight / 2.4)
                );
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
            {
                m_isShiftPressed = true;
            }
            else
            {
                m_isShiftPressed = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && !m_keysPressed.Contains(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_keysPressed.Contains(Keys.Enter))
            {
                Storage.Name.playerName = m_name.ToString();
                return GameStateEnum.GameInstructions;
            }

            updateName(Keyboard.GetState().GetPressedKeys());

            m_timeSinceLastBackspace += (float) gameTime.ElapsedGameTime.TotalMilliseconds;

            // update the list of pressed keys
            // we use a list of pressed keys because when you type you are often pressing the
            // next key before another has already gone all the way up. Saving the list of 
            // pressed keys lets us get keyboard input at an acceptable rate
            m_keysPressed.Clear();
            foreach (Keys key in Keyboard.GetState().GetPressedKeys())
            {
                m_keysPressed.Add(key);
            }

            return GameStateEnum.ChooseName;
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
                m_fontMenu,
                m_instructions_text1,
                m_instructions_position1,
                Color.Black);

            m_spriteBatch.DrawString(
                m_fontMenu,
                m_instructions_text2,
                m_instructions_position2,
                Color.Black);

            m_spriteBatch.Draw(
                m_texPanel,
                m_rectInputBox,
                Color.White);

            m_spriteBatch.DrawString(
                m_fontMenu, 
                m_name, 
                new Vector2(m_rectInputBox.X + 15, m_rectInputBox.Y + 15), 
                Color.Black);

            m_spriteBatch.End();
        }

        public void updateName(Keys[] pressedKeys)
        {
            foreach (Keys key in pressedKeys)
            {
                if (key == Keys.Back && m_name.Length > 0 && m_timeSinceLastBackspace >= 150)
                {
                    m_timeSinceLastBackspace = 0f;
                    m_name.Length--;
                }
                else if (!m_keysPressed.Contains(key) && m_name.Length < 7)
                {
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        char keyChar = (char)key;
                        if (!m_isShiftPressed)
                        {
                            keyChar = (char) (key.ToString()[0] + 32);
                        }
                        m_name.Append(keyChar);
                    }
                    else if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        m_name.Append((char) key);
                    }
                }
            }
        }
    }
}
