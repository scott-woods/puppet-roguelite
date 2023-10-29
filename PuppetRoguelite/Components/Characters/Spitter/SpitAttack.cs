﻿using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Spitter
{
    public class SpitAttack : EnemyAction<Spitter>, IUpdatable
    {
        bool _hasLaunchedProjectile = false;

        public SpitAttack(Spitter enemy) : base(enemy)
        {

        }

        protected override IEnumerator StartAction()
        {
            _hasLaunchedProjectile = false;
            _enemy.Animator.Play("Attack", Nez.Sprites.SpriteAnimator.LoopMode.Once);
            _enemy.Animator.OnAnimationCompletedEvent += OnAttackAnimationCompleted;
            yield break;
        }

        public void Update()
        {
            //if in the running state
            if (_state == EnemyActionState.Running)
            {
                //if haven't launched yet
                if (!_hasLaunchedProjectile)
                {
                    //if in the correct animation frame
                    if (_enemy.Animator.CurrentAnimationName == "Attack" && _enemy.Animator.CurrentFrame == 3)
                    {
                        //play sound
                        Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Spitter_fire, 1.4f);
                        
                        //get direction to player
                        var dir = PlayerController.Instance.Entity.Position - _enemy.Entity.Position;
                        dir.Normalize();

                        CreateProjectile(dir);

                        var leftRotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(30));
                        var leftRotatedDir = Vector2.Transform(dir, leftRotationMatrix);
                        CreateProjectile(leftRotatedDir);

                        var rightRotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-30));
                        var rightRotatedDir = Vector2.Transform(dir, rightRotationMatrix);
                        CreateProjectile(rightRotatedDir);

                        _hasLaunchedProjectile = true;
                    }
                }
            }
        }

        void CreateProjectile(Vector2 dir)
        {
            var ent = new PausableEntity("spit-projectile");
            Entity.Scene.AddEntity(ent);
            ent.SetPosition(Entity.Position);
            ent.AddComponent(new SpitAttackProjectile(dir));
        }

        void OnAttackAnimationCompleted(string animationName)
        {
            _enemy.Animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;
            _state = EnemyActionState.Succeeded;
        }

        public override void Abort()
        {
            base.Abort();

            _enemy.Animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;
        }
    }
}
