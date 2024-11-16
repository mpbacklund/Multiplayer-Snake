using Microsoft.Xna.Framework;
using System;

namespace Client.Particles
{
    public class Particle
    {
        public Particle(Vector2 center, Vector2 direction, float speed, Vector2 size, float lifetime)
        {
            this.name = m_nextName++;
            this.center = center;
            this.direction = direction;
            this.speed = speed;
            this.size = size;
            this.lifetime = lifetime;

            this.rotation = 0;
        }

        public bool update(TimeSpan elapsedTime)
        {
            // Update how long it has been alive
            alive += (float) elapsedTime.TotalMilliseconds;

            // Update its center
            center.X += (float)(elapsedTime.TotalMilliseconds * speed * direction.X);
            center.Y += (float)(elapsedTime.TotalMilliseconds * speed * direction.Y);

            // Rotate proportional to its speed
            rotation += (speed / 0.5f);

            // Return true if this particle is still alive
            return alive < lifetime;
        }

        public long name;
        public Vector2 size;
        public Vector2 center;
        public float rotation;
        private Vector2 direction;
        private float speed;
        private float lifetime;
        private float alive = 0f;
        private static long m_nextName = 0;
    }
}
