using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class EntranceTrigger : Component, ITriggerListener
    {
        TmxObject _tmxObject;
        DungeonRoom _room;

        Collider _collider;

        public EntranceTrigger(TmxObject tmxObject, DungeonRoom dungeonRoom)
        {
            _tmxObject = tmxObject;
            _room = dungeonRoom;
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = new BoxCollider(_tmxObject.Width, _tmxObject.Height);
            _collider.LocalOffset = new Microsoft.Xna.Framework.Vector2(_tmxObject.Width / 2, _tmxObject.Height / 2);
            _collider.IsTrigger = true;
            _collider.CollidesWithLayers = (int)PhysicsLayers.Collider;
            Entity.AddComponent(_collider);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (int)PhysicsLayers.Collider)
            {
                if (other.HasComponent<PlayerController>())
                {
                    _room.HandleEntranceTriggered();
                }
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            //throw new NotImplementedException();
        }
    }
}
