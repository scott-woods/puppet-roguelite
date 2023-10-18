using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    /// <summary>
    /// includes a collider with a trigger to transition to a new scene
    /// </summary>
    public class ExitArea : TiledComponent, ITriggerListener
    {
        Collider _collider;

        Type _sceneType;
        Vector2 _size;

        public ExitArea(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
            if (tmxObject.Properties.TryGetValue("TargetScene", out var targetScene))
            {
                _sceneType = Type.GetType("PuppetRoguelite.Scenes." + targetScene);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(_size.X, _size.Y));
            _collider.IsTrigger = true;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.HasComponent<PlayerController>())
            {
                Game1.SceneManager.ChangeScene(_sceneType);
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            //throw new NotImplementedException();
        }
    }
}
