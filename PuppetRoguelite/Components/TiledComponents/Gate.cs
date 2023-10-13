using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class Gate : TiledComponent
    {
        Collider _collider;

        public Gate(TmxObject gateObj, string mapId) : base(gateObj, mapId)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
            //_collider.LocalOffset = new Vector2(TmxObject.Width / 2, TmxObject.Height / 2);
            Entity.AddComponent(_collider);
            _collider.SetEnabled(false);

            TmxObject.Visible = true;
        }

        public void Lock()
        {
            _collider.SetEnabled(true);
            TmxObject.Visible = true;
        }

        public void Unlock()
        {
            _collider.SetEnabled(false);
            TmxObject.Visible = false;
        }
    }
}
