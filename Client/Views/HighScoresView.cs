using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using Client.Storage;

namespace Client.Views
{
    public class HighScoresView : GameStateView
    {
        private SpriteFont m_font;
        private SpriteFont m_fontMenuSelect;

        private List<ScoreData> m_highScores = new List<ScoreData>();

        private Texture2D m_texBackground;
        private Texture2D m_texPanel;

        private Rectangle m_rectBackground;
        private Rectangle m_rectPanel;

        private bool loading = false;

        public override void initializeSession()
        {
            loadHighScores();
        }

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
                m_graphics.PreferredBackBufferWidth / 8,
                m_graphics.PreferredBackBufferHeight / 6,
                (int)(m_graphics.PreferredBackBufferWidth * .75f),
                (int)(m_graphics.PreferredBackBufferHeight * .75f)
                );
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.HighScores;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.Draw(m_texBackground, m_rectBackground, Color.White);

            m_spriteBatch.Draw(m_texPanel, m_rectPanel, Color.White * 0.6f);


            float bottom = 150;
            Vector2 stringSize = m_fontMenuSelect.MeasureString("High Scores");
            m_spriteBatch.DrawString(
                m_fontMenuSelect,
                "High Scores",
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, bottom),
                Color.Blue);
            bottom += 20 + stringSize.Y;

            int place = 1;
            foreach (var score in m_highScores )
            {
                bottom = drawMenuItem(
                m_font,
                $"{place.ToString()}. {score.Name}",
                score.Score.ToString(),
                score.TimeStamp.ToShortDateString(),
                bottom,
                Color.Black);
                place++;
            }

            m_spriteBatch.End();
        }

        private float drawMenuItem(SpriteFont font, string name, string score, string date, float y, Color color)
        {
            Vector2 stringSize = font.MeasureString(name);

            m_spriteBatch.DrawString(
                font,
                name,
                new Vector2(m_graphics.PreferredBackBufferWidth / 6, y),
                color);

            m_spriteBatch.DrawString(
                font,
                score,
                new Vector2(m_graphics.PreferredBackBufferWidth / 2, y),
                color);

            m_spriteBatch.DrawString(
                font,
                date,
                new Vector2(m_graphics.PreferredBackBufferWidth * 0.65f, y),
                color);

            return y + stringSize.Y;
        }

        public override void update(GameTime gameTime)
        {
        }

        #region Save Scores
        private void loadHighScores()
        {
            lock (this)
            {
                if (!this.loading)
                {
                    this.loading = true;
                    // Yes, I know the result is not being saved, I dont' need it
                    var result = finalizeLoadHighScoresAsync();
                    result.Wait();

                }
            }
        }

        private async Task finalizeLoadHighScoresAsync()
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        if (storage.FileExists("HighScores.json"))
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Open))
                            {
                                if (fs != null)
                                {
                                    DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(ScoreData));

                                    // get all of the high scores for the game
                                    using (StreamReader reader = new StreamReader(fs))
                                    {
                                        m_highScores.Clear();

                                        // Read lines from the file until the end
                                        string line;
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(line));
                                            ScoreData scoreData = (ScoreData)mySerializer.ReadObject(ms);
                                            m_highScores.Add(scoreData);
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
