using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Entities
{
    public class PausableEntity : Entity
    {
        bool _paused = false;

        public PausableEntity(string name) : base(name)
        {

        }

        public void TogglePaused()
        {
            _paused = !_paused;
        }

        public override void Update()
        {
            if (!_paused)
            {
                base.Update();
            }
        }
    }
}
