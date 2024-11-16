using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Views
{
    public interface IGameState
    {
        void initializeSession();
        void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics);
        void loadContent(ContentManager contentManager);
        GameStateEnum processInput(GameTime gameTime);
        void update(GameTime gameTime);
        void render(GameTime gameTime);
    }
}