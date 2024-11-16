using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using Snake.Storage;
using System.Diagnostics.Metrics;

namespace Client.Views
{
    public class ControlsView : GameStateView
    {
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;

        private Texture2D m_texBackground;
        private Texture2D m_texPanel;

        private Rectangle m_rectBackground;
        private Rectangle m_rectPanel;

        private Dictionary<Shared.Components.Input.Type, Keys> m_typeToKey = new Dictionary<Shared.Components.Input.Type, Keys>();
        private enum ControlsChoices
        {
            Up,
            Down, 
            Left, 
            Right,
            None
        }

        private bool loading = false;
        private bool saving = false;

        private ControlsChoices m_currentSelection = ControlsChoices.Up;
        private ControlsChoices m_currentChanging = ControlsChoices.None;
        private bool m_waitForKeyRelease = true;

        public override void initializeSession()
        {
            loadKeys();

            // if this is the first time playing the game, no keyData will be stored.
            // load default keys into the game and save them to the external file
            if (m_typeToKey.Count == 0)
            {
                m_typeToKey.Add(Shared.Components.Input.Type.Up, Keys.Up);
                m_typeToKey.Add(Shared.Components.Input.Type.Down, Keys.Down);
                m_typeToKey.Add(Shared.Components.Input.Type.Left, Keys.Left);
                m_typeToKey.Add(Shared.Components.Input.Type.Right, Keys.Right);

                saveKeys();
            }

            m_waitForKeyRelease = true;
        }

        public override void loadContent(ContentManager contentManager)
        {
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
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
                m_graphics.PreferredBackBufferWidth / 8,
                m_graphics.PreferredBackBufferHeight / 6,
                3* m_graphics.PreferredBackBufferWidth / 4,
                2 * m_graphics.PreferredBackBufferHeight / 3
                );
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }
            if (!m_waitForKeyRelease)
            {
                if (m_currentChanging == ControlsChoices.None)
                {
                    navigateMenu();
                }
                else
                {
                    changeControls();
                }

            }
            else if (Keyboard.GetState().GetPressedKeyCount() == 0)
            {
                m_waitForKeyRelease = false;
            }

            return GameStateEnum.Controls;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.Draw(m_texBackground, m_rectBackground, Color.White);

            m_spriteBatch.Draw(m_texPanel, m_rectPanel, Color.White * 0.6f);

            float bottom = drawMenuItem(
                m_fontMenu,
                "Press Enter on the control you want \n to change, then press the new key.",
                150, // Adjust vertical position to separate from previous item
                Color.Black); // Choose a color for the message

            bottom = drawMenuItem(
                m_currentSelection == ControlsChoices.Up ? m_fontMenuSelect : m_fontMenu,
                $"UP: {(m_currentChanging == ControlsChoices.Up ? "Press a new key" : m_typeToKey[Shared.Components.Input.Type.Up].ToString())}",
                bottom + 20,
                m_currentSelection == ControlsChoices.Up ? Color.Blue : Color.Black);
            bottom = drawMenuItem(
                m_currentSelection == ControlsChoices.Down ? m_fontMenuSelect : m_fontMenu,
                $"DOWN: {(m_currentChanging == ControlsChoices.Down ? "Press a new key" : m_typeToKey[Shared.Components.Input.Type.Down].ToString())}",
                bottom,
                m_currentSelection == ControlsChoices.Down ? Color.Blue : Color.Black);
            bottom = drawMenuItem(
                m_currentSelection == ControlsChoices.Left ? m_fontMenuSelect : m_fontMenu,
                $"LEFT: {(m_currentChanging == ControlsChoices.Left ? "Press a new key" : m_typeToKey[Shared.Components.Input.Type.Left].ToString())}",
                bottom,
                m_currentSelection == ControlsChoices.Left ? Color.Blue : Color.Black);
            bottom = drawMenuItem(
                m_currentSelection == ControlsChoices.Right ? m_fontMenuSelect : m_fontMenu,
                $"RIGHT: {(m_currentChanging == ControlsChoices.Right ? "Press a new key" : m_typeToKey[Shared.Components.Input.Type.Right].ToString())}",
                bottom,
                m_currentSelection == ControlsChoices.Right ? Color.Blue : Color.Black);

            bottom = drawMenuItem(
                m_fontMenu,
                "Press ESC to return to the main menu.",
                bottom + 20, // Adjust vertical position to separate from previous item
                Color.Black); // Choose a color for the message

            m_spriteBatch.End();
        }


        public override void update(GameTime gameTime)
        {
        }

        private void navigateMenu()
        {
            // Arrow keys to navigate the menu
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && m_currentSelection != ControlsChoices.Right)
            {
                m_currentSelection = m_currentSelection + 1;
                m_waitForKeyRelease = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && m_currentSelection != ControlsChoices.Up)
            {
                m_currentSelection = m_currentSelection - 1;
                m_waitForKeyRelease = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                m_waitForKeyRelease = true;
                m_currentChanging = m_currentSelection;
            }
        }

        private void changeControls()
        {
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            
            if (pressedKeys.Length > 0)
            {
                if (m_currentChanging == ControlsChoices.Up)
                {
                    m_typeToKey[Shared.Components.Input.Type.Up] = pressedKeys[0];
                }
                if (m_currentChanging == ControlsChoices.Down)
                {
                    m_typeToKey[Shared.Components.Input.Type.Down] = pressedKeys[0];
                }
                if (m_currentChanging == ControlsChoices.Left)
                {
                    m_typeToKey[Shared.Components.Input.Type.Left] = pressedKeys[0];
                }
                if (m_currentChanging == ControlsChoices.Right)
                {
                    m_typeToKey[Shared.Components.Input.Type.Right] = pressedKeys[0];
                }
                m_waitForKeyRelease = true;
                m_currentChanging = ControlsChoices.None;
                saveKeys();
            }
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

        #region Save Controls
        private void saveKeys()
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
                        // clear the old file
                        using (IsolatedStorageFileStream fs = storage.OpenFile("Keys.json", FileMode.Create))
                        {

                        }

                        // add the keys to the file
                        foreach (var member in m_typeToKey)
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("Keys.json", FileMode.Append))
                            {
                                if (fs != null)
                                {
                                    DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(KeyData));
                                    
                                    KeyData keyData = new KeyData(member.Key, member.Value);
                                    
                                    mySerializer.WriteObject(fs, keyData);
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

        #region Load Controls
        private void loadKeys()
        {
            lock (this)
            {
                if (!this.loading)
                {
                    this.loading = true;
                    // Yes, I know the result is not being saved, I dont' need it
                    var result = finalizeLoadKeysAsync();
                    result.Wait();

                }
            }
        }

        private async Task finalizeLoadKeysAsync()
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
                                        m_typeToKey.Clear();

                                        // Read lines from the file until the end
                                        string line;
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(line));
                                            Snake.Storage.KeyData keyData = ((Snake.Storage.KeyData)mySerializer.ReadObject(ms));
                                            m_typeToKey.Add(keyData.Direction, keyData.Key);
                                        }
                                    }
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
    }
}
