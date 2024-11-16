using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems
{
    public class Animation : Shared.Systems.System
    {

        public Animation() : base(typeof(Components.Animation))
        {
        }

        public override void update(TimeSpan elapsedTime)
        {
            foreach (var entity in m_entities.Values)
            {
                entity.get<Components.Animation>().animationTimer += (float)elapsedTime.TotalMilliseconds;
                if (entity.get<Components.Animation>().animationTimer >= entity.get<Components.Animation>().spriteTime[entity.get<Components.Animation>().subImageIndex])
                {
                    entity.get<Components.Animation>().animationTimer -= entity.get<Components.Animation>().spriteTime[entity.get<Components.Animation>().subImageIndex];
                    entity.get<Components.Animation>().subImageIndex++;
                    entity.get<Components.Animation>().subImageIndex = (uint)(entity.get<Components.Animation>().subImageIndex % entity.get<Components.Animation>().spriteTime.Length);
                }
            }
        }
    }
}
