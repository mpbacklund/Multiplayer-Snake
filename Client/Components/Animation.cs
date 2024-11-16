using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Components
{
    public class Animation : Shared.Components.Component
    {
        public Animation(uint subImageIndex, uint[] spriteTime) 
        { 
            this.subImageIndex = subImageIndex;
            this.spriteTime = spriteTime;
            animationTimer = 0;
        }

        public uint subImageIndex {  get; set; }
        public float animationTimer {  get; set; }
        public uint[] spriteTime {  get; set; }
        
    }
}
