using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.Tiled;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class AreaTrigger : TiledComponent, IUpdatable
    {
        public event Action OnTriggered;

        //components
        Collider _collider;

        //misc
        string _eventName;
        bool _shouldDestroy = false;

        public AreaTrigger(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            switch (TmxObject.ObjectType)
            {
                case TmxObjectType.Basic:
                case TmxObjectType.Ellipse:
                case TmxObjectType.Tile:
                    _collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
                    break;
                case TmxObjectType.Polygon:
                    var points = new List<Vector2>(TmxObject.Points);
                    _collider = Entity.AddComponent(new PolygonCollider(points.ToArray()));
                    if (_collider.LocalOffset == Vector2.Zero)
                    {
                        _collider.SetLocalOffset(new Vector2(TmxObject.Width / 2, TmxObject.Height / 2));
                    }
                    break;
            }

            //collider
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.Trigger);
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.PlayerCollider);
            Entity.AddComponent(_collider);

            if (TmxObject.Properties != null && TmxObject.Properties.Count > 0 )
            {
                if (TmxObject.Properties.TryGetValue("EventName", out string eventName))
                {
                    _eventName = eventName;
                }
                if (TmxObject.Properties.TryGetValue("ShouldDestroy", out string shouldDestroy))
                {
                    _shouldDestroy = true;
                }
            }
        }

        public void Update()
        {
            var colliders = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
            if (colliders.Count > 0)
            {
                HandleTriggered();
                if (_shouldDestroy)
                    Entity.Destroy();
            }
        }

        public virtual void HandleTriggered()
        {
            if (!string.IsNullOrWhiteSpace(_eventName))
                Game1.StartCoroutine(Game1.EventManager.PlayEvent(_eventName));

            OnTriggered?.Invoke();
        }
    }
}
