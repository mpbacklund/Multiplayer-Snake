using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Client.Particles
{
    public class ParticleSystem
    {
        private Dictionary<long, Particle> m_particles = new Dictionary<long, Particle>();
        public Dictionary<long, Particle>.ValueCollection particles { get { return m_particles.Values; } }
        private Shared.MyRandom m_random = new Shared.MyRandom();

        private Vector2 m_center;
        private int m_sizeMean; // pixels
        private int m_sizeStdDev;   // pixels
        private float m_speedMean;  // pixels per millisecond
        private float m_speedStDev; // pixels per millisecond
        private float m_lifetimeMean; // milliseconds
        private float m_lifetimeStdDev; // milliseconds
        private float m_systemTotalDuration; // milliseconds
        private float m_systemCurrentDuration;
        private Texture2D m_texParticle;

        public ParticleSystem(Vector2 center, int sizeMean, int sizeStdDev, float speedMean, float speedStdDev, int lifetimeMean, int lifetimeStdDev, float systemLifetime, Texture2D texture)
        {
            m_center = center;
            m_sizeMean = sizeMean;
            m_sizeStdDev = sizeStdDev;
            m_speedMean = speedMean;
            m_speedStDev = speedStdDev;
            m_lifetimeMean = lifetimeMean;
            m_lifetimeStdDev = lifetimeStdDev;
            m_systemTotalDuration = systemLifetime;
            m_systemCurrentDuration = 0f;
            m_texParticle = texture;
        }

        private Particle create()
        {
            float size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
            var p = new Particle(
                    m_center,
                    m_random.nextCircleVector(),
                    (float)m_random.nextGaussian(m_speedMean, m_speedStDev),
                    new Vector2(size, size),
                    (float) (m_random.nextGaussian(m_lifetimeMean, m_lifetimeStdDev))); ;

            return p;
        }

        public bool update(TimeSpan elapsedTime)
        {
            // Update existing particles
            List<long> removeMe = new List<long>();
            foreach (Particle p in m_particles.Values)
            {
                if (!p.update(elapsedTime))
                {
                    removeMe.Add(p.name);
                }
            }

            // Remove dead particles
            foreach (long key in removeMe)
            {
                m_particles.Remove(key);
            }

            m_systemCurrentDuration += (float) elapsedTime.TotalMilliseconds;
            if (! (m_systemCurrentDuration > m_systemTotalDuration))
            {
                // Generate some new particles
                for (int i = 0; i < 8; i++)
                {
                    var particle = create();
                    m_particles.Add(particle.name, particle);
                }
            }

            return m_particles.Count != 0;
        }

        public void draw(SpriteBatch spriteBatch, Vector2 viewPortStart)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Rectangle r = new Rectangle(0, 0, 0, 0);
            Vector2 centerTexture = new Vector2(m_texParticle.Width / 2, m_texParticle.Height / 2);
            foreach (Particle particle in m_particles.Values)
            {
                r.X = (int)(particle.center.X - viewPortStart.X);
                r.Y = (int)(particle.center.Y - viewPortStart.Y);
                r.Width = (int)particle.size.X;
                r.Height = (int)particle.size.Y;

                spriteBatch.Draw(
                    m_texParticle,
                    r,
                    null,
                    Color.White,
                    particle.rotation,
                    centerTexture,
                    SpriteEffects.None,
                    0);
            }

            spriteBatch.End();
        }
    }
}