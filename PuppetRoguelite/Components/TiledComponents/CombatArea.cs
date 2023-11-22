using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class CombatArea : TiledComponent
    {
        PolygonCollider _collider;

        public CombatArea(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var points = new List<Vector2>(TmxObject.Points);
            _collider = Entity.AddComponent(new PolygonCollider(points.ToArray()));
            if (_collider.LocalOffset == Vector2.Zero)
            {
                _collider.SetLocalOffset(new Vector2(TmxObject.Width / 2, TmxObject.Height / 2));
            }
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.CombatArea);
        }

        public static bool IsPointInCombatArea(Vector2 position)
        {
            //try to get combat area collider
            var combatAreaCollider = Physics.OverlapCircle(position, .5f, 1 << (int)PhysicsLayers.CombatArea);
            if (combatAreaCollider == null) return false;
            else
            {
                //get dungenerator
                var dungenerator = Game1.Scene.GetSceneComponent<Dungenerator>();

                //if dungenerator is null but we are overlapping a combat area, return true
                //no dungenerator means only one map, no need to check that we're on the right map
                if (dungenerator == null) return true;
                else
                {
                    //get room node by position
                    var roomNode = dungenerator.GetNodeByWorldPosition(position);

                    //get combat area component from collider
                    var combatArea = combatAreaCollider.GetComponent<CombatArea>();

                    //if map entities match, the position is valid
                    if (combatArea.MapEntity == roomNode.MapEntity)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
