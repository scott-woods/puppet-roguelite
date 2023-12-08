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
        bool _noSword = false;

        //components
        public SpriteAnimator SpriteAnimator;
        YSorter _ySorter;
        public OriginComponent OriginComponent;
        public VelocityComponent VelocityComponent;
        Mover _mover;
        public SpriteFlipper SpriteFlipper;

        public PlayerSim(Vector2 direction, bool noSword = false)
        {
            _direction = direction;
            _noSword = noSword;
        }

        public PlayerSim(bool noSword = false)
        {
            _noSword = noSword;
        }

        public override void Initialize()
        {
            base.Initialize();

            SpriteAnimator = Entity.AddComponent(PlayerController.Instance.SpriteAnimator.Clone() as SpriteAnimator);
            SpriteAnimator.SetColor(new Color(255, 255, 255, 128));

            VelocityComponent = Entity.AddComponent(PlayerController.Instance.VelocityComponent.Clone() as VelocityComponent);

            var offset = PlayerController.Instance.Collider.LocalOffset;
            OriginComponent = Entity.AddComponent(new OriginComponent(offset));

            _ySorter = Entity.AddComponent(new YSorter(SpriteAnimator, OriginComponent));

            _mover = Entity.AddComponent(new Mover());

            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(SpriteAnimator, VelocityComponent));

            Entity.AddComponent(new Shadow(SpriteAnimator, new Vector2(0, 6), Vector2.One));
        }

        public void Update()
        {
            if (Entity.GetType().BaseType != typeof(PlayerAction))
            {
                var extension = _noSword ? "NoSword" : "";
                //handle animation
                var animation = $"IdleDown{extension}";
                if (_direction.X != 0)
                {
                    animation = $"Idle{extension}";
                }
                else if (_direction.Y != 0)
                {
                    animation = _direction.Y >= 0 ? $"IdleDown{extension}" : $"IdleUp{extension}";
                }
                if (!SpriteAnimator.IsAnimationActive(animation))
                {
                    SpriteAnimator.Play(animation);
                }
            }
        }

        public void Idle(Direction direction)
        {
            VelocityComponent.SetDirection(direction);

            var extension = _noSword ? "NoSword" : "";

            var animation = "";
            switch (direction)
            {
                case Direction.Up:
                    animation = $"IdleUp{extension}";
                    break;
                case Direction.Down:
                    animation = $"IdleDown{extension}";
                    break;
                case Direction.Left:
                    animation = $"Idle{extension}";
                    break;
                case Direction.Right:
                    animation = $"Idle{extension}";
                    break;
            }

            if (SpriteAnimator.CurrentAnimationName != animation)
            {
                SpriteAnimator.Play(animation);
            }
        }
    }
}
