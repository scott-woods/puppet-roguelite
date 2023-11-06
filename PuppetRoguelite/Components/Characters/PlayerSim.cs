using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Components.Shared;

namespace PuppetRoguelite.Components.Characters
{
    public class PlayerSim : Component, IUpdatable
    {
        Vector2 _direction = new Vector2(1, 0);

        //components
        SpriteAnimator _spriteAnimator;
        YSorter _ySorter;
        OriginComponent _originComponent;

        public PlayerSim(Vector2 direction)
        {
            _direction = direction;
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            AddAnimations();
        }

        void AddComponents()
        {
            _spriteAnimator = Entity.AddComponent(new SpriteAnimator());
            _spriteAnimator.SetColor(new Color(Color.White, 128));

            _originComponent = Entity.AddComponent(new OriginComponent(new Vector2(0, 12)));

            _ySorter = Entity.AddComponent(new YSorter(_spriteAnimator, _originComponent));
        }

        void AddAnimations()
        {
            //idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            _spriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            _spriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            _spriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            _spriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));
        }

        public void Update()
        {
            //handle animation
            var animation = "IdleRight";
            if (_direction.X != 0)
            {
                animation = _direction.X >= 0 ? "IdleRight" : "IdleLeft";
            }
            else if (_direction.Y != 0)
            {
                animation = _direction.Y >= 0 ? "IdleDown" : "IdleUp";
            }
            if (!_spriteAnimator.IsAnimationActive(animation))
            {
                _spriteAnimator.Play(animation);
            }
        }

        public override void OnRemovedFromEntity()
        {
            Entity.RemoveComponent(_spriteAnimator);
            Entity.RemoveComponent(_originComponent);
            Entity.RemoveComponent(_ySorter);
            base.OnRemovedFromEntity();
        }
    }
}
