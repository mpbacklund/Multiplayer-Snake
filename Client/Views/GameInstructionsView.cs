using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Snake.Storage;
using System.Runtime.Intrinsics.X86;

namespace Client.Views
{
    internal class GameInstructionsView : GameStateView
    {
        ContentManager m_content;
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;

        private Texture2D m_texBackground;
        private Texture2D m_texPanel;

        private Rectangle m_rectBackground;
        private Rectangle m_rectPanel;

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

        private bool m_waitForKeyRelease = true;

        private Dictionary<Shared.Components.Input.Type, Keys> m_typeToKey = new Dictionary<Shared.Components.Input.Type, Keys>();
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

        public override void loadContent(ContentManager content)
        {
            m_fontMenu = content.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = content.Load<SpriteFont>("Fonts/menu-select");
            m_texBackground = content.Load<Texture2D>("Textures/menu2");
            m_texPanel = content.Load<Texture2D>("Textures/panel");

            m_rectBackground = new Rectangle(
                0,
                0,
                m_graphics.PreferredBackBufferWidth,
                m_graphics.PreferredBackBufferHeight
                );

            m_rectPanel = new Rectangle(
                m_graphics.PreferredBackBufferWidth / 16,
                m_graphics.PreferredBackBufferHeight / 8,
                (int)(m_graphics.PreferredBackBufferWidth * 0.875),
                (int)(m_graphics.PreferredBackBufferHeight * 0.75)
                );
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (! m_waitForKeyRelease)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    m_waitForKeyRelease = true;
                    return GameStateEnum.ChooseName;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    m_waitForKeyRelease = true;
                    return GameStateEnum.GamePlay;
                }
            }
            else if (Keyboard.GetState().GetPressedKeyCount() == 0)
            {
                m_waitForKeyRelease = false;
            }

            return GameStateEnum.GameInstructions;
        }

        public override void update(GameTime gameTime)
        {
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.Draw(m_texBackground, m_rectBackground, Color.White);

            m_spriteBatch.Draw(m_texPanel, m_rectPanel, Color.White * 0.6f);

            float bottom = drawMenuItem(
                m_fontMenuSelect,
                "Controls:",
                110, // Adjust vertical position to separate from previous item
                Color.Blue); // Choose a color for the message

            bottom = drawMenuItem(
                m_fontMenu,
                $"The snake will go whichever direction you press.",
                bottom + 20,
                Color.Black);

            bottom = drawMenuItem(
                m_fontMenu,
                $"Use multiple keys to move diagonally.",
                bottom,
                Color.Black);

            bottom = drawMenuItem(
                m_fontMenu,
                $"UP: {m_typeToKey[Shared.Components.Input.Type.Up].ToString()}",
                bottom+ 20,
                Color.Black);

            bottom = drawMenuItem(
                m_fontMenu,
                $"DOWN: {m_typeToKey[Shared.Components.Input.Type.Down].ToString()}",
                bottom,
                Color.Black);
            bottom = drawMenuItem(
                m_fontMenu,
                $"LEFT: {m_typeToKey[Shared.Components.Input.Type.Left].ToString()}",
                bottom,
                Color.Black);

            bottom = drawMenuItem(
                m_fontMenu,
                $"RIGHT: {m_typeToKey[Shared.Components.Input.Type.Right].ToString()}",
                bottom,
                Color.Black);

            bottom = drawMenuItem(
                m_fontMenu,
                "Press ESC to exit the game at any time",
                bottom + 20, // Adjust vertical position to separate from previous item
                Color.Black); // Choose a color for the message

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
                        using (IsolatedStorageFileStream fs = storage.OpenFile("Keys.json", FileMode.Create))
                        {

                        }
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
                                    // get the 5 highest scores and return them in HighScores
                                }
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // Ideally show something to the user, but this is demo code :)
                    }
                }

                this.loading = false;
            });
        }
        #endregion
    }
}
