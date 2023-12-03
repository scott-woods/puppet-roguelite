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
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Characters.Player;

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
        public OriginComponent OriginComponent;
        public VelocityComponent VelocityComponent;
        Mover _mover;
        public SpriteFlipper SpriteFlipper;

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

            _spriteAnimator = Entity.AddComponent(PlayerController.Instance.SpriteAnimator.Clone() as SpriteAnimator);
            _spriteAnimator.SetColor(new Color(255, 255, 255, 128));

            VelocityComponent = Entity.AddComponent(PlayerController.Instance.VelocityComponent.Clone() as VelocityComponent);

            var offset = PlayerController.Instance.Collider.LocalOffset;
            OriginComponent = Entity.AddComponent(new OriginComponent(offset));

            _ySorter = Entity.AddComponent(new YSorter(_spriteAnimator, OriginComponent));

            _mover = Entity.AddComponent(new Mover());

            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(_spriteAnimator, VelocityComponent));

            Entity.AddComponent(new Shadow(_spriteAnimator, new Vector2(0, 6), Vector2.One));
        }

        public void Update()
        {
            if (Entity.GetType().BaseType != typeof(PlayerAction))
            {
                //handle animation
                var animation = "IdleDown";
                if (_direction.X != 0)
                {
                    animation = "Idle";
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
                    animation = "Idle";
                    break;
                case Direction.Right:
                    animation = "Idle";
                    break;
            }

            if (_spriteAnimator.CurrentAnimationName != animation)
            {
                _spriteAnimator.Play(animation);
            }
        }
    }
}
