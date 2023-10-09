using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class Gate : Component
    {
        TmxObject _gateObj;

        Collider _collider;

        public Gate(TmxObject gateObj)
        {
            _gateObj = gateObj;
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = new BoxCollider(_gateObj.Width, _gateObj.Height);
            _collider.LocalOffset = new Vector2(_gateObj.Width / 2, _gateObj.Height / 2);
            Entity.AddComponent(_collider);
            _collider.SetEnabled(false);

            _gateObj.Visible = false;
        }

        public void Lock()
        {
            _collider.SetEnabled(true);
            _gateObj.Visible = true;
        }

        public void Unlock()
        {
            _collider.SetEnabled(false);
            _gateObj.Visible = false;
        }
    }
}
