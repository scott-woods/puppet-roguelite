using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    /// <summary>
    /// includes a collider with a trigger to transition to a new scene
    /// </summary>
    public class ExitArea : Component, ITriggerListener
    {
        Collider _collider;

        Type _sceneType;
        Vector2 _size;
        Vector2 _offset;

        public ExitArea(Type sceneType, Vector2 size, Vector2 offset)
        {
            _sceneType = sceneType;
            _size = size;
            _offset = offset;
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(_size.X, _size.Y));
            _collider.LocalOffset += _offset;
            _collider.IsTrigger = true;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.HasComponent<PlayerController>())
            {
                Game1.SceneManager.ChangeScene(Activator.CreateInstance(_sceneType) as Scene);
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            //throw new NotImplementedException();
        }
    }
}
