using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Entities;

namespace PuppetRoguelite.Components.Characters.Enemies.Spitter
{
    public class SpitAttack : EnemyAction<Spitter>, IUpdatable
    {
        bool _hasLaunchedProjectile = false;

        public SpitAttack(Spitter enemy) : base(enemy)
        {

        }

        protected override void StartAction()
        {
            _hasLaunchedProjectile = false;
            _enemy.Animator.Play("Attack", Nez.Sprites.SpriteAnimator.LoopMode.Once);
            _enemy.Animator.OnAnimationCompletedEvent += OnAttackAnimationCompleted;
        }

        public void Update()
        {
            //if in the running state
            if (_state == EnemyActionState.Running)
            {
                //if haven't launched yet
                if (!_hasLaunchedProjectile)
                {
                    //ensure animation is running
                    if (_enemy.Animator.CurrentAnimationName != "Attack")
                        _enemy.Animator.Play("Attack");

                    //if in the correct animation frame
                    if (_enemy.Animator.CurrentAnimationName == "Attack" && _enemy.Animator.CurrentFrame == 3)
                    {
                        //play sound
                        Game1.AudioManager.PlaySound(Content.Audio.Sounds.Spitter_fire);

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
