using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class DungeonRoom : Component
    {
        RoomNode _node;
        bool _isCleared = false;
        string _mapString;

        TmxMap _map;
        TiledMapRenderer _mapRenderer;

        List<Entity> _triggerEntities = new List<Entity>();
        List<Gate> _gates = new List<Gate>();

        public DungeonRoom(RoomNode node, string mapString)
        {
            _node = node;
            _mapString = mapString;
        }

        public override void Initialize()
        {
            base.Initialize();

            //map renderer
            _map = Entity.Scene.Content.LoadTiledMap(_mapString);
            _mapRenderer = Entity.AddComponent(new TiledMapRenderer(_map, "collision"));
            _mapRenderer.SetLayersToRender(new[] { "floor", "details", "gates" });
            _mapRenderer.RenderLayer = 10;

            //gates
            var gateObjGroup = _map.GetObjectGroup("gates");
            if (gateObjGroup != null)
            {
                gateObjGroup.Visible = true;
                foreach (var gateObj in gateObjGroup.Objects)
                {
                    gateObj.Y -= 16;
                    var ent = Entity.Scene.CreateEntity(gateObj.Name);
                    ent.SetPosition(Entity.Position + new Vector2((int)gateObj.X, (int)gateObj.Y));
                    var gate = ent.AddComponent(new Gate(gateObj));

                    _gates.Add(gate);
                }
            }

            //triggers
            var triggerObjGroup = _map.GetObjectGroup("entrance-triggers");
            if (triggerObjGroup != null)
            {
                for (int i = 0; i < triggerObjGroup.Objects.Count; i++)
                {
                    var trigger = triggerObjGroup.Objects[i];

                    var ent = Entity.Scene.CreateEntity(trigger.Name);
                    ent.SetPosition(Entity.Position + new Vector2((int)trigger.X, (int)trigger.Y));
                    ent.AddComponent(new EntranceTrigger(trigger, this));
                    _triggerEntities.Add(ent);
                }
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            foreach(var ent in _triggerEntities)
            {
                ent.Destroy();
            }
        }

        public void HandleEntranceTriggered()
        {
            if (!_isCleared)
            {
                foreach (var gate in _gates)
                {
                    gate.Lock();
                }

                foreach(var ent in _triggerEntities)
                {
                    ent.Destroy();
                }
            }
        }
    }
}
