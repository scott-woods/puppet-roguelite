using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Enemies;
using PuppetRoguelite.Components.Effects;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Tools;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    [PlayerActionInfo("Chain Lightning", 3, PlayerActionCategory.Attack)]
    public class ChainLightning : PlayerAction
    {
        //data
        const int _damage = 3;
        const int _damageAddedPerChain = 1;
        const int _hitboxRadius = 12;
        const int _hitboxDistFromPlayer = 12;
        const float _strikeInterval = .1f;
        const int _chainRadius = 100;
        List<int> _hitboxActiveFrames = new List<int>() { 0 };

        //components
        PlayerSim _playerSim;
        SpriteAnimator _animator;
        CircleHitbox _hitbox;
        DirectionByMouse _dirByMouse;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop;
        ICoroutine _executionCoroutine;
        ICoroutine _attackCoroutine;

        //misc
        List<Entity> _hitEntities = new List<Entity>();
        int _currentChain = 0;
        bool _hasFinishedSwinging = false;
        Direction _direction = Direction.Right;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim());

            _hitbox = AddComponent(new CircleHitbox(_damage, _hitboxRadius));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.SetEnabled(false);

            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));
            var texture = Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_attack);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);
            _animator.AddAnimation("ChainLightningUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 32, 35));
            _animator.AddAnimation("ChainLightningDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 23, 26));
            _animator.AddAnimation("ChainLightningLeft", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 14, 15, 16, 16 }, true));
            _animator.AddAnimation("ChainLightningRight", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 5, 6, 7, 7 }, true));

            _dirByMouse = AddComponent(new DirectionByMouse());

            Log.Debug("Starting ChainLightning SimulationLoop");
            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        IEnumerator SimulationLoop()
        {
            while (State == PlayerActionState.Preparing)
            {
                //idle for a moment
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                _playerSim.VelocityComponent.SetDirection(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.2f);

                //attack
                Log.Debug("Starting ChainLightning Attack Coroutine");
                _animator.Speed = 1f;
                _attackCoroutine = _coroutineManager.StartCoroutine(Attack());
                yield return _attackCoroutine;
                _attackCoroutine = null;
                Log.Debug("Finished ChainLightning Attack Coroutine");

                //idle again
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.1f);
            }
        }

        public override void Execute()
        {
            base.Execute();

            Log.Debug("Starting ChainLightning Execution Coroutine");

            _executionCoroutine = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            //set animator speed and color
            _animator.SetColor(Color.White);
            _animator.Speed = 1.3f;

            //attack and wait for completion
            Log.Debug("Starting ChainLightning Attack Coroutine");
            _attackCoroutine = _coroutineManager.StartCoroutine(Attack());
            yield return _attackCoroutine;
            _attackCoroutine = null;
            Log.Debug("Finished ChainLightning Attack Coroutine");

            //ensure that hitbox is disabled after attack completion
            _hitbox.SetEnabled(false);

            //get all enemies
            var allEnemies = Scene.FindComponentsOfType<EnemyBase>();

            //loop to chain to other enemies
            Log.Debug("Starting ChainLightning Chain Loop");
            bool finished = false;
            while (!finished)
            {
                //increment chain
                Log.Debug("Incrementing ChainLightning Chain");
                _currentChain++;

                //get all enemies that haven't been hit
                var unhitEnemies = allEnemies.Where(e => !_hitEntities.Contains(e.Entity));

                //get list of next entities to hit
                var entitiesToHit = new List<Entity>();
                foreach (var ent in _hitEntities)
                {
                    foreach (var unhitEnemy in unhitEnemies)
                    {
                        //if enemy hasn't already been added to next list
                        if (!entitiesToHit.Contains(unhitEnemy.Entity))
                        {
                            //if within distance
                            if (Vector2.Distance(unhitEnemy.Entity.Position, ent.Position) <= _chainRadius)
                            {
                                entitiesToHit.Add(unhitEnemy.Entity);
                            }
                        }
                    }
                }

                //if no more entities to hit, break
                if (entitiesToHit.Count == 0)
                {
                    Log.Debug("No more Entities to hit, breaking ChainLightning Loop");
                    break;
                }

                //wait for strike interval
                Log.Debug("ChainLightning: starting strike interval");
                yield return Coroutine.WaitForSeconds(_strikeInterval);
                Log.Debug("ChainLightning: finished strike interval");

                //strike next entities
                Log.Debug("ChainLightning: striking next entities");
                foreach (var entity in entitiesToHit)
                {
                    //play sound
                    Game1.AudioManager.PlaySound(Content.Audio.Sounds.Small_lightning);

                    //create lightning strike effect
                    var effectEntity = Scene.CreateEntity("lightning-effect");
                    if (entity.TryGetComponent<BoxHitbox>(out var boxHitbox))
                        effectEntity.SetPosition(boxHitbox.Bounds.Center);
                    else if (entity.TryGetComponent<CircleHitbox>(out var circleHitbox))
                        effectEntity.SetPosition(circleHitbox.Bounds.Center);
                    else if (entity.TryGetComponent<PolygonHitbox>(out var polygonHitbox))
                        effectEntity.SetPosition(polygonHitbox.Bounds.Center);
                    else
                        effectEntity.SetPosition(entity.Position);
                    var effectComponent = effectEntity.AddComponent(new HitEffectComponent(HitEffects.Lightning2));
                    var effectHitbox = effectEntity.AddComponent(new CircleHitbox(_damage + _currentChain * _damageAddedPerChain, 1));
                    Flags.SetFlagExclusive(ref effectHitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
                    Flags.SetFlagExclusive(ref effectHitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
                    effectComponent.PlayAnimation();
                }
                Log.Debug("ChainLightning: finished striking next entities");

                //add newly hit entities to hitEntities list
                _hitEntities.AddRange(entitiesToHit);
            }

            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            _coroutineManager.Update();

            if (State == PlayerActionState.Preparing)
            {
                //handle confirm
                if (Input.LeftMouseButtonPressed)
                {
                    Log.Debug("stopping chain lightning because confirm button clicked");
                    Reset();
                    HandlePreparationFinished(Position);
                }

                //restart loop if changed direction
                if (_dirByMouse.CurrentDirection != _dirByMouse.PreviousDirection)
                {
                    Log.Debug("stopping chain lightning because changed direction");
                    Reset();
                    _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
                }

                //handle hitbox offset based on direction
                _direction = _dirByMouse.CurrentDirection;
                Vector2 offset = Vector2.Zero;
                switch (_dirByMouse.CurrentDirection)
                {
                    case Direction.Up:
                        offset = new Vector2(0, -1);
                        break;
                    case Direction.Down:
                        offset = new Vector2(0, 1);
                        break;
                    case Direction.Left:
                        offset = new Vector2(-1, 0);
                        break;
                    case Direction.Right:
                        offset = new Vector2(1, 0);
                        break;
                }
                offset *= _hitboxDistFromPlayer;
                _hitbox.SetLocalOffset(offset);
            }
            else if (!_hasFinishedSwinging)
            {
                //enable hitbox if in valid frame
                List<string> validAnims = new List<string> { "Slash", "SlashDown", "SlashUp" };
                if (validAnims.Contains(_animator.CurrentAnimationName))
                {
                    if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                    {
                        if (!_hitbox.Enabled)
                        {
                            _hitbox.SetEnabled(true);
                        }
                    }
                    else _hitbox.SetEnabled(false);
                }
                else _hitbox.SetEnabled(false);

                //check for hit
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_hitbox, _hitbox.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    //loop through colliding hurtboxes
                    foreach (var collider in colliders)
                    {
                        //if hit entities doesn't already of this entity, add it
                        if (!_hitEntities.Contains(collider.Entity))
                        {
                            _hitEntities.Add(collider.Entity);
                        }
                    }
                }
            }
        }

        IEnumerator Attack()
        {
            _hasFinishedSwinging = false;

            var animation = "Slash";
            switch (_direction)
            {
                case Direction.Left:
                case Direction.Right:
                    animation = "Slash"; break;
                case Direction.Up:
                    animation = "SlashUp"; break;
                case Direction.Down:
                    animation = "SlashDown"; break;
            }
            _animator.Play(animation, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAnimationFinished;

            bool hasPlayedSound = false;
            while (!_hasFinishedSwinging)
            {
                if (State == PlayerActionState.Executing)
                {
                    if (!hasPlayedSound)
                    {
                        if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                        {
                            hasPlayedSound = true;
                            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Big_lightning);
                            Game1.AudioManager.PlaySound(Content.Audio.Sounds._31_swoosh_sword_1);

                            var effectEntity = Scene.CreateEntity("lightning-effect");
                            effectEntity.SetPosition(Position + _hitbox.LocalOffset);
                            var effectComponent = effectEntity.AddComponent(new HitEffectComponent(HitEffects.Lightning1));
                            effectComponent.PlayAnimation();
                        }
                    }
                }

                yield return null;
            }

            Log.Debug("ChainLightning: finished attack");
        }

        void OnAnimationFinished(string animation)
        {
            Log.Debug("animation finished " + Name);
            _animator.OnAnimationCompletedEvent -= OnAnimationFinished;
            _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
            _hasFinishedSwinging = true;
        }

        public override void Reset()
        {
            Log.Debug("Resetting ChainLightning");

            //handle coroutines
            _simulationLoop?.Stop();
            _simulationLoop = null;
            _executionCoroutine?.Stop();
            _executionCoroutine = null;
            _attackCoroutine?.Stop();
            _attackCoroutine = null;

            //fields
            _hitEntities.Clear();
            _hasFinishedSwinging = false;
            _currentChain = 0;

            //disable hitbox
            _hitbox.SetEnabled(false);

            //animator
            _animator.Stop();
            _animator.OnAnimationCompletedEvent -= OnAnimationFinished;
        }
    }
}
