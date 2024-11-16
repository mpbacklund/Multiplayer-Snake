using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;

namespace Client.Systems
{
    /// <summary>
    /// This system imitates the ECS system, but does not extend it.
    /// It does not need to since particle systems are objects, not entities.
    /// You may not like this, but it works really well. 
    /// </summary>
    public class Particles
    {
        private List<ParticleSystem> m_particleSystems;
        private Texture2D m_texFoodParticle;
        private Texture2D m_texDeathParticle;
        

        public Particles() 
        {
            m_particleSystems = new List<Client.Particles.ParticleSystem>();
        }

        public void loadContent(ContentManager content)
        {
            m_texDeathParticle = content.Load<Texture2D>("Textures/deathParticle");
            m_texFoodParticle = content.Load<Texture2D>("Textures/foodParticle");
        }

        public void update(TimeSpan elapsedTime)
        {
            var particleSystemsCopy = new List<ParticleSystem>(m_particleSystems);

            foreach (ParticleSystem system in particleSystemsCopy)
            {
                bool isActive = system.update(elapsedTime);
                if (!isActive)
                {
                    m_particleSystems.Remove(system);
                }
            }
        }

        public void draw(SpriteBatch spriteBatch, Vector2 viewPortStart)
        {
            foreach (ParticleSystem system in m_particleSystems)
            {
                system.draw(spriteBatch, viewPortStart);
            }
        }

        public void playerDeath(Vector2 position)
        {
            var newSystem = new ParticleSystem(
                position,
                15, 4, // size
                0.07f, 0.05f, // speed
                500, 150, // particle lifetime
                500, // system lifetime
                m_texDeathParticle
                );
            m_particleSystems.Add(newSystem);
        }

        public void foodEaten(Vector2 position)
        {
            var newSystem = new ParticleSystem(
                position,
                6, 2, // size
                0.10f, 0.1f, // speed
                150, 30, // particle lifetime
                100, // system lifetime
                m_texFoodParticle
                );
            m_particleSystems.Add(newSystem);
        }
    }
}
