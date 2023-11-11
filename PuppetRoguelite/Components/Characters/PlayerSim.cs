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
using PuppetRoguelite.Components.PlayerActions;

namespace PuppetRoguelite.Components.Characters
{
    /// <summary>
    /// stripped down version of the player with just a few basic components
    /// </summary>
    public class PlayerSim : Component, IUpdatable
    {
        Vector2 _direction = new Vector2(1, 0);

        //components
        SpriteAnimator _spriteAnimator;
        YSorter _ySorter;
        OriginComponent _originComponent;
        Mover _mover;

        public PlayerSim(Vector2 direction)
        {
            _direction = direction;
        }

        public PlayerSim()
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _spriteAnimator = Entity.AddComponent(new SpriteAnimator());
            //_spriteAnimator.SetColor(new Color(Color.White, 128));

            _originComponent = Entity.AddComponent(new OriginComponent(new Vector2(0, 12)));

            _ySorter = Entity.AddComponent(new YSorter(_spriteAnimator, _originComponent));

            _mover = Entity.AddComponent(new Mover());

            AddAnimations();
        }

        void AddAnimations()
        {
            //idle
            var idleTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            _spriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            _spriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            _spriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            _spriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));

            //run
            var runTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            var runFps = 13;
            _spriteAnimator.AddAnimation("RunRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 0, 9), runFps);
            _spriteAnimator.AddAnimation("RunLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 10, 19), runFps);
            _spriteAnimator.AddAnimation("RunDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 20, 27), runFps);
            _spriteAnimator.AddAnimation("RunUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 30, 37), runFps);

            //hurt
            var hurtTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_hurt);
            var hurtSprites = Sprite.SpritesFromAtlas(hurtTexture, 64, 64);
            _spriteAnimator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 0, 4));
            _spriteAnimator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 5, 9));

            //death
            var deathTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_death);
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 64, 64);
            _spriteAnimator.AddAnimation("DeathRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 0, 9));
            _spriteAnimator.AddAnimation("DeathLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 10, 19));

            //dash
            var dashTexture = Game1.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var dashSprites = Sprite.SpritesFromAtlas(dashTexture, 64, 64);
            _spriteAnimator.AddAnimation("DashRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 4, 4));
            _spriteAnimator.AddAnimation("DashLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 13, 13));
            _spriteAnimator.AddAnimation("DashDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 26, 26));
            _spriteAnimator.AddAnimation("DashUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 35, 35));
        }

        public void Update()
        {
            if (Entity.GetType().BaseType != typeof(PlayerAction))
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
        }

        public void Idle(Direction direction)
        {
            var animation = "";
            switch (direction)
            {
                case Direction.Up:
                    animation = "IdleUp";
                    break;
                case Direction.Down:
                    animation = "IdleDown";
                    break;
                case Direction.Left:
                    animation = "IdleLeft";
                    break;
                case Direction.Right:
                    animation = "IdleRight";
                    break;
            }

            if (_spriteAnimator.CurrentAnimationName != animation)
            {
                _spriteAnimator.Play(animation);
            }
        }

        //public override void OnDisabled()
        //{
        //    base.OnDisabled();

        //    _spriteAnimator.Stop();
        //}

        //public override void OnRemovedFromEntity()
        //{
        //    Entity.RemoveComponent(_spriteAnimator);
        //    Entity.RemoveComponent(_originComponent);
        //    Entity.RemoveComponent(_ySorter);
        //    Entity.RemoveComponent(_mover);
        //    base.OnRemovedFromEntity();
        //}
    }
}
