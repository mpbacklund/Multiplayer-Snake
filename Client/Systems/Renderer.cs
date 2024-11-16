
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Systems
{
    public class Renderer : Shared.Systems.System
    {
        Texture2D m_texBackground;
        Texture2D m_texPanel;
        Texture2D m_tail;
        Texture2D m_enemyTail;
        private SpriteFont m_font;
        private SpriteBatch m_spriteBatch;

        public Renderer(SpriteBatch spriteBatch) :
            base(
                typeof(Components.Sprite),
                typeof(Shared.Components.Position),
                typeof(Shared.Components.Size)
                )
        {
            m_spriteBatch = spriteBatch;
        }

        public void loadContent(ContentManager contentManager)
        {
            m_texBackground = contentManager.Load<Texture2D>("Textures/background");
            m_font = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_texPanel = contentManager.Load<Texture2D>("Textures/panel");
            m_tail = contentManager.Load<Texture2D>("Textures/tail");
            m_enemyTail = contentManager.Load<Texture2D>("Textures/tail_enemy");
        }

        public override void update(TimeSpan elapsedTime) { }

        public void update(TimeSpan elapsedTime, Vector2 screenSize, Vector2 gameSize, Entity me, Dictionary<uint, uint> heads, Dictionary<uint, uint> highScores, int highestPosition, Client.Systems.Particles particles)
        {
            Vector2 center = me.get<Shared.Components.Position>().position; // center of screen in game coords
            Vector2 topLeftCornerWindow = center - new Vector2(screenSize.X / 2, screenSize.Y / 2); // top left corner of screen in game coords

            m_spriteBatch.Begin();

            renderBackground(topLeftCornerWindow, gameSize, screenSize);

            foreach (var key in m_entities.Keys.OrderByDescending(k => k))
            {
                var entity = m_entities[key];
                if (entity.contains<Shared.Components.Food>())
                {
                    renderFood(entity, topLeftCornerWindow);
                }
                else
                {
                    renderSnakes(entity, me.get<SnakeId>().id, heads, topLeftCornerWindow);
                }
            }

            foreach (var id in heads.Values)
            {
                renderNames(m_entities[id], topLeftCornerWindow);
            }

            m_spriteBatch.End();

            particles.draw(m_spriteBatch, topLeftCornerWindow);

            m_spriteBatch.Begin();

            if (me.get<Shared.Components.SnakeData>().state != Shared.Components.SnakeData.snakeState.dead)
            {
                renderScores(me, highScores, screenSize);
            }
            else
            {
                renderDeathScreen(topLeftCornerWindow, me, highestPosition, screenSize);
            }

            m_spriteBatch.End();
        }

        public void renderScores(Entity me, Dictionary<uint, uint> highScores, Vector2 screenSize)
        {
            // render My score at top of screen
            string myScore = me.get<Shared.Components.SnakeData>().score.ToString();
            m_spriteBatch.DrawString(
                m_font,
                myScore,
                new Vector2(screenSize.X / 2 - m_font.MeasureString(myScore).X / 2, 100), // Use the calculated position for the text
                Color.Black,
                0f, // Rotation angle (if any)
                Vector2.Zero, // Origin
                1.5f, // Scale
                SpriteEffects.None,
                0.6f // Layer depth
                );

            // render high scores background box
            m_spriteBatch.Draw(
                m_texPanel,
                new Rectangle(
                    (int) (screenSize.X - (screenSize.X / 3) - 15),
                    15,
                    (int) screenSize.X / 3,
                    (int)screenSize.Y / 3
                    ),
                Color.White * 0.5f
                );

            float position = 30f;
            // render scores
            m_spriteBatch.DrawString(
                m_font,
                "High Scores",
                new Vector2(screenSize.X - (screenSize.X / 3), position),
                Color.Black,
                0f, // Rotation (in radians)
                Vector2.Zero, // Origin
                0.6f, // Scale
                SpriteEffects.None,
                0f);
            position += m_font.MeasureString("High Scores").Y * 0.6f;

            int place = 1;
            foreach (uint id in highScores.Values )
            {
                position = drawScoreItem(
                    m_font,
                    $"{place}: {m_entities[id].get<Shared.Components.Name>().name}",
                    position,
                    m_entities[id].get<Shared.Components.SnakeData>().score.ToString(),
                    screenSize
                    ); ;
                place++;
            }
        }

        private float drawScoreItem(SpriteFont font, string text, float y, string score, Vector2 screenSize)
        {
            Vector2 stringSize = font.MeasureString(text);

            // Scale the font to half size
            float scaleFactor = 0.5f;
            m_spriteBatch.DrawString(
                font,
                text,
                new Vector2(screenSize.X - (screenSize.X / 3), y),
                Color.Black,
                0f, // Rotation (in radians)
                Vector2.Zero, // Origin
                scaleFactor, // Scale
                SpriteEffects.None,
                0f);

            Vector2 scoreSize = font.MeasureString(score);
            m_spriteBatch.DrawString(
                font,
                score,
                new Vector2(screenSize.X - (scoreSize.X * scaleFactor) - 30, y),
                Color.Black,
                0f, // Rotation (in radians)
                Vector2.Zero, // Origin
                scaleFactor, // Scale
                SpriteEffects.None,
                0f);

            return y + (stringSize.Y * scaleFactor); // Adjusting for the scaled size
        }

        public void renderBackground(Vector2 viewPortStart, Vector2 gameSize, Vector2 screenSize)
        {
            int tileSize = 1000;

            // render background tiles (only the ones the player can see)
            Vector2 rectTile = new Vector2();
            rectTile.X = -1 * (viewPortStart.X % tileSize);
            rectTile.Y = -1 * (viewPortStart.Y % tileSize);

            for (float x = rectTile.X; x < screenSize.X; x += tileSize)
            {
                for (float y = rectTile.Y; y < screenSize.Y; y += tileSize)
                {
                    Rectangle tempRectTile = new Rectangle((int)x, (int)y, tileSize, tileSize);

                    if (viewPortStart.Y + y < gameSize.Y && viewPortStart.X + x < gameSize.X)
                    {
                        m_spriteBatch.Draw(m_texBackground, tempRectTile, Color.White);
                    }
                }
            }

            // render outside boarder for game
            Rectangle rectangle = new Rectangle(
                (int)(-1 * viewPortStart.X),
                (int)(-1 * viewPortStart.Y),
                (int)gameSize.X,
                (int)2);
            drawBorder(rectangle);

            rectangle = new Rectangle(
                (int)(-1 * viewPortStart.X),
                (int)(-1 * viewPortStart.Y),
                (int)2,
                (int)gameSize.Y);
            drawBorder(rectangle);

            rectangle = new Rectangle(
                (int)(-1 * viewPortStart.X),
                (int)(gameSize.Y - viewPortStart.Y),
                (int)gameSize.X,
                (int)2);
            drawBorder(rectangle);

            rectangle = new Rectangle(
                (int)(gameSize.X - viewPortStart.X),
                (int)(-1 * viewPortStart.Y),
                (int)2,
                (int)gameSize.Y);
            drawBorder(rectangle);
        }

        private void drawBorder(Rectangle rectangle)
        {
            m_spriteBatch.Draw(m_texPanel, rectangle, Color.Red);
        }

        public void renderFood(Entity entity, Vector2 viewPortStart)
        {
            var position = entity.get<Shared.Components.Position>().position;
            var orientation = entity.get<Shared.Components.Position>().orientation;
            var size = entity.get<Shared.Components.Size>().size;
            var texture = entity.get<Components.Sprite>().texture;
            var animation = entity.get<Components.Animation>();

            Rectangle rectPosition = new Rectangle(
                (int)(position.X - viewPortStart.X),
                (int)(position.Y - viewPortStart.Y),
                (int)size.X,
                (int)size.Y);

            int subImageWidth = texture.Width / animation.spriteTime.Length;
            Rectangle rectAnimationFrame = new Rectangle(
                (int)animation.subImageIndex * subImageWidth,
                0,
                subImageWidth,
                texture.Height
                );

            m_spriteBatch.Draw(
                texture,
                rectPosition,
                rectAnimationFrame,
                Color.White,
                orientation,
                new Vector2(subImageWidth / 2, texture.Height / 2),
                SpriteEffects.None, 
                0.5f
                );
        }

        public void renderSnakes(Entity entity, uint mySnakeId, Dictionary<uint, uint> heads, Vector2 viewPortStart)
        {
            var position = entity.get<Shared.Components.Position>().position;
            var orientation = entity.get<Shared.Components.Position>().orientation;
            var size = entity.get<Shared.Components.Size>().size;
            var texture = entity.get<Components.Sprite>().texture;
            var texCenter = entity.get<Components.Sprite>().center;

            // we make this check to prevent errors from occuring while a snake is being removed from the game
            if (heads.ContainsKey(entity.get<SnakeId>().id))
            {
                // if entity is a tail change the texture to a tail
                var head = m_entities[heads[entity.get<Shared.Components.SnakeId>().id]];
                if (entity.id == head.get<Shared.Components.SnakeData>().tailId)
                {
                    if (head.get<Shared.Components.SnakeId>().id == entity.get<Shared.Components.SnakeId>().id)
                    {
                        if (entity.get<Shared.Components.SnakeId>().id == mySnakeId)
                        {
                            texture = m_tail;
                        }
                        else
                        {
                            texture = m_enemyTail;
                        }

                    }
                }
            }

            Rectangle rectangle = new Rectangle(
                (int)(position.X - viewPortStart.X),
                (int)(position.Y - viewPortStart.Y),
                (int)size.X,
                (int)size.Y);

            m_spriteBatch.Draw(
                texture,
                rectangle,
                null,
                Color.White,
                orientation,
                texCenter,
                SpriteEffects.None,
                0.5f);
        }

        public void renderNames(Entity entity, Vector2 viewPortStart)
        {
            var position = entity.get<Shared.Components.Position>().position;

            Vector2 namePosition = new Vector2(
                (int)(position.X - viewPortStart.X) - (m_font.MeasureString(entity.get<Shared.Components.Name>().name).X / 4),
                (int)(position.Y - viewPortStart.Y) - entity.get<Shared.Components.Size>().size.Y // Place the text above the entity
                );

            m_spriteBatch.DrawString(
                m_font,
                entity.get<Shared.Components.Name>().name,
                namePosition, // Use the calculated position for the text
                Color.White,
                0f, // Rotation angle (if any)
                Vector2.Zero, // Origin
                0.4f, // Scale
                SpriteEffects.None,
                0.6f // Layer depth
                );
        }

        public void renderDeathScreen(Vector2 viewPortStart, Entity me, int topPosition ,Vector2 screenSize)
        {
            m_spriteBatch.Draw(
                m_texPanel,
                new Rectangle(
                    0,
                    (int) screenSize.Y / 4,
                    (int) screenSize.X,
                    (int) screenSize.Y / 2
                    ),
                Color.White * 0.5f
                );

            float bottom = drawDeathScreenItem(
                $"Highest Leaderboard Position: {topPosition}",
                screenSize.Y * 0.25f, // Adjust vertical position to separate from previous item
                screenSize); // Choose a color for the message
            bottom = drawDeathScreenItem(
                $"Score: {me.get<Shared.Components.SnakeData>().score}",
                bottom, // Adjust vertical position to separate from previous item
                screenSize); // Choose a color for the message
            bottom = drawDeathScreenItem(
                $"kills: {me.get<Shared.Components.SnakeData>().kills}",
                bottom, // Adjust vertical position to separate from previous item
                screenSize); // Choose a color for the message
            bottom = drawDeathScreenItem(
                $"Press Escape to return to the main menu",
                2 * screenSize.Y / 3, // Adjust vertical position to separate from previous item
                screenSize); // Choose a color for the message
        }

        private float drawDeathScreenItem(string text, float y, Vector2 screenSize)
        {
            Vector2 stringSize = m_font.MeasureString(text);
            m_spriteBatch.DrawString(
                m_font,
                text,
                new Vector2(screenSize.X / 2 - stringSize.X / 2, y),
                Color.Black);

            return y + stringSize.Y;
        }
    }
}
