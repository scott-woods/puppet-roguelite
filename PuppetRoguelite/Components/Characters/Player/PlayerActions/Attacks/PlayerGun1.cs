using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    public class PlayerGun1 : Component, IUpdatable
    {
        //constants
        const int _offset = 10;

        //components
        SpriteRenderer _renderer;

        //params
        PlayerSim _playerSim;
        bool _simulation;

        //misc
        Vector2 _direction;

        public PlayerGun1(PlayerSim playerSim, Vector2 direction, bool simulation)
        {
            _playerSim = playerSim;
            _direction = direction;
            _simulation = simulation;
        }

        public override void Initialize()
        {
            base.Initialize();

            _renderer = Entity.AddComponent(new SpriteRenderer());
            if (_simulation)
                _renderer.SetColor(new Color(255, 255, 255, 128));
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Gun1);
            var sprite = new Sprite(texture);
            _renderer.SetSprite(sprite);

            //set direction here so gun is properly rotated
            SetDirection(_direction);
        }

        public void Update()
        {
            _renderer.RenderLayer = _playerSim.SpriteAnimator.RenderLayer - 1;
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction;

            //rotate sprite
            Entity.SetRotation((float)Math.Atan2(direction.Y, direction.X));

            //rotate position around player sim
            Entity.Position = _playerSim.Entity.Position + (direction * _offset);

            //flip if necessary
            _renderer.FlipY = direction.X < 0;
        }

        public void Fire()
        {
            var projectileEntity = Entity.Scene.CreateEntity("gun-projectile");
            var projectile = projectileEntity.AddComponent(new PlayerGunProjectile(_direction, _simulation));
            projectileEntity.SetPosition(Entity.Position);
        }
    }
}
