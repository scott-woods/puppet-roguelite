using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class Dollah : Component, IUpdatable
    {
        float _initialVelocityY = -200f;
        Vector2 _velocity;
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();
        bool _isLaunching = false;
        bool _isMovingToPlayer = false;
        float _timeSinceLaunced = 0f;
        float _launchDuration = .75f;
        float _initialSpeedTowardsPlayer = 150f;
        float _speedTowardsPlayer = 0f;
        float _speedIncreaseFactor = 1.05f;

        SpriteRenderer _spriteRenderer;
        ProjectileMover _mover;
        CircleCollider _circleCollider;
        YSorter _ySorter;
        OriginComponent _originComponent;

        public override void Initialize()
        {
            base.Initialize();

            _circleCollider = Entity.AddComponent(new CircleCollider());
            Flags.SetFlagExclusive(ref _circleCollider.PhysicsLayer, (int)PhysicsLayers.Pickup);
            Flags.SetFlagExclusive(ref _circleCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _circleCollider.IsTrigger = true;
            _circleCollider.SetEnabled(false);

            _mover = Entity.AddComponent(new ProjectileMover());

            _spriteRenderer = Entity.AddComponent(new SpriteRenderer());

            _originComponent = Entity.AddComponent(new OriginComponent(_circleCollider));

            _ySorter = Entity.AddComponent(new YSorter(_spriteRenderer, _originComponent));

            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Tilesets.Dungeon_prison_props);
            var sprite = new Sprite(texture, new Rectangle(80, 240, 16, 16));
            _spriteRenderer.SetSprite(sprite);
        }

        public void Launch()
        {
            _timeSinceLaunced = 0f;

            var velocityX = Nez.Random.Range(-75f, 75f);
            //if (Nez.Random.Chance(.5f))
            //{
            //    velocityX = Nez.Random.Range(-75f, -40f);
            //}
            //else
            //{
            //    velocityX = Nez.Random.Range(40f, 75f);
            //}
            _velocity = new Vector2(velocityX, _initialVelocityY);

            _isLaunching = true;
        }

        public void Update()
        {
            if (_isLaunching)
            {
                _timeSinceLaunced += Time.DeltaTime;

                if (_timeSinceLaunced >= _launchDuration)
                {
                    _velocity = Vector2.Zero;
                    _isLaunching = false;
                    _isMovingToPlayer = true;
                    _circleCollider.SetEnabled(true);
                    _speedTowardsPlayer = _initialSpeedTowardsPlayer;
                    return;
                }

                _velocity.Y += Physics.Gravity.Y * Time.DeltaTime;

                var movement = _velocity * Time.DeltaTime;

                _subPixelV2.Update(ref movement);
                _mover.Move(movement);
            }
            else if (_isMovingToPlayer)
            {
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_circleCollider, _circleCollider.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    _isMovingToPlayer = false;
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Dollah_pickup, .4f);
                    PlayerController.Instance.DollahInventory.AddDollahs(1);
                    Entity.Destroy();
                    return;
                }

                _velocity.Y += Physics.Gravity.Y * Time.DeltaTime;

                //if within certain distance to player, just stick to them
                if (Math.Abs(Vector2.Distance(Entity.Position, PlayerController.Instance.Entity.Position)) <= 4)
                {
                    Entity.Position = PlayerController.Instance.Entity.Position;
                }
                else
                {
                    _speedTowardsPlayer *= _speedIncreaseFactor;
                    var dir = PlayerController.Instance.Entity.Position - Entity.Position;
                    dir.Normalize();
                    var movement = dir * _speedTowardsPlayer * Time.DeltaTime;

                    _subPixelV2.Update(ref movement);
                    _mover.Move(movement);
                }
            }
        }
    }
}
