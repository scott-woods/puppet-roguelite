using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class InvincibilityComponent : Component
    {
        bool _isInvincible = false;
        public bool IsInvincible
        {
            get { return _isInvincible; }
        }
        float _duration;

        public InvincibilityComponent(float duration)
        {
            _duration = duration;
        }

        public void Activate()
        {
            _isInvincible = true;
            Core.Schedule(_duration, timer => _isInvincible = false);
        }
    }
}
