using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies.Ghoul
{
    public class GhoulAttack : EnemyAction<Ghoul>, IUpdatable
    {
        //components
        CircleHitbox _hitbox;

        //misc
        List<int> _hitboxActiveFrames = new List<int> { 2 };
        Vector2 _hitboxOffset = new Vector2(9, 0);

        public GhoulAttack(Ghoul enemy) : base(enemy)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _hitbox = Entity.AddComponent(new CircleHitbox(1, 8));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _hitbox.SetEnabled(false);
        }

        protected override void StartAction()
        {
            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Ghoul_claw);
            _enemy.Animator.Play("Attack", Nez.Sprites.SpriteAnimator.LoopMode.Once);
            _enemy.Animator.OnAnimationCompletedEvent += OnAnimationCompleted;
        }

        public override void Abort()
        {
            base.Abort();

            _enemy.Animator.OnAnimationCompletedEvent -= OnAnimationCompleted;
            _hitbox.SetEnabled(false);
        }

        public void Update()
        {
            if (_state == EnemyActionState.Running)
            {
                if (_enemy.VelocityComponent.Direction.X >= 0)
                {
                    _hitbox.SetLocalOffset(_hitboxOffset);
                }
                else _hitbox.SetLocalOffset(-_hitboxOffset);
                if (_enemy.Animator.IsAnimationActive("Attack") && _hitboxActiveFrames.Contains(_enemy.Animator.CurrentFrame))
                {
                    _hitbox.SetEnabled(true);
                }
                else _hitbox.SetEnabled(false);
            }
        }

        void OnAnimationCompleted(string animationName)
        {
            _enemy.Animator.OnAnimationCompletedEvent -= OnAnimationCompleted;
            HandleActionFinished();
        }
    }
}
