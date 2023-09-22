using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class HurtComponent : Component
    {
        public void PlayHurtEffect()
        {
            if (Entity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                animator.SetColor(Color.Red);
                Core.Schedule(.5f, timer => animator.SetColor(Color.White));
            }
        }
    }
}
