using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Effects;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Attacks
{
    [PlayerActionInfo("Chain Lightning", 3, PlayerActionCategory.Attack)]
    public class ChainLightning : PlayerAction
    {
        //data
        const int _damage = 3;
        const int _damageAddedPerChain = 1;
        const int _hitboxRadius = 12;
        const int _hitboxDistFromPlayer = 12;
        const float _strikeInterval = .2f;
        const int _chainRadius = 100;

        //components
        PlayerSim _playerSim;
        SpriteAnimator _animator;
        CircleHitbox _hitbox;
        DirectionByMouse _dirByMouse;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop;
        ICoroutine _executionCoroutine;

        //misc
        List<int> _hitboxActiveFrames = new List<int>() { 0, 1 };
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

            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White, 128));
            var texture = Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);
            _animator.AddAnimation("ChainLightningUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 32, 35));
            _animator.AddAnimation("ChainLightningDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 23, 26));
            _animator.AddAnimation("ChainLightningLeft", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 14, 15, 16, 16 }, true));
            _animator.AddAnimation("ChainLightningRight", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 5, 6, 7, 7 }, true));

            _dirByMouse = AddComponent(new DirectionByMouse());

            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        IEnumerator SimulationLoop()
        {
            while (State == PlayerActionState.Preparing)
            {
                //idle for a moment
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.2f);

                //attack
                _animator.Speed = 1f;
                yield return Attack();

                //idle again
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.1f);
            }
        }

        public override void Execute()
        {
            base.Execute();

            _executionCoroutine = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            _animator.SetColor(Color.White);
            _animator.Speed = 1.3f;

            Debug.Log("starting attack");
            yield return Attack();
            _hitbox.SetEnabled(false);

            var count = _hitEntities.Count;
            var lastHitEntities = new List<Entity>();
            lastHitEntities.AddRange(_hitEntities);
            var allEnemies = Scene.FindComponentsOfType<Enemy>();
            Debug.Log("starting interval checks. Count: " + count.ToString());
            while (count > 0)
            {
                _currentChain += 1;

                //wait for strike interval
                Debug.Log("waiting for strike interval");
                yield return Coroutine.WaitForSeconds(_strikeInterval);
                Debug.Log("strike interval passed");

                //get next entities to hit
                Debug.Log("starting loop through hit entities");
                List<Entity> nextEntities = new List<Entity>();
                foreach (var entity in lastHitEntities) //loop through hit entities
                {
                    var enemiesToRemove = new List<Enemy>();
                    foreach (var enemy in allEnemies)
                    {
                        //if already hit
                        if (nextEntities.Contains(enemy.Entity) || _hitEntities.Contains(enemy.Entity))
                        {
                            enemiesToRemove.Add(enemy);
                            continue;
                        }

                        //if within distance
                        if (Vector2.Distance(enemy.Entity.Position, entity.Position) <= _chainRadius)
                        {
                            nextEntities.Add(enemy.Entity);
                            _hitEntities.Add(enemy.Entity);
                            enemiesToRemove.Add(enemy);
                        }
                    }

                    foreach (var enemy in enemiesToRemove)
                    {
                        allEnemies.Remove(enemy);
                    }
                }

                Debug.Log("Starting loop through enemies to strike");
                //strike next entities
                foreach(var entity in nextEntities)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Small_lightning, 1f);

                    var effectEntity = Scene.CreateEntity("lightning-effect");
                    effectEntity.SetPosition(entity.Position);
                    var effectComponent = effectEntity.AddComponent(new HitEffectComponent(HitEffects.Lightning2));
                    var effectHitbox = effectEntity.AddComponent(new CircleHitbox(_damage + (_currentChain * _damageAddedPerChain), 1));
                    Flags.SetFlagExclusive(ref effectHitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
                    Flags.SetFlagExclusive(ref effectHitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
                    effectComponent.PlayAnimation();
                }

                Debug.Log("clearing last hit entities");
                lastHitEntities.Clear();
                lastHitEntities.AddRange(nextEntities);
                count = lastHitEntities.Count;
                Debug.Log("Finished strike interval. Count: " + count.ToString());
            }

            Debug.Log("Handling execution finished");
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
                    _simulationLoop?.Stop();
                    _animator.Stop();
                    HandlePreparationFinished(Position);
                }

                //restart loop if changed direction
                if (_dirByMouse.CurrentDirection != _dirByMouse.PreviousDirection)
                {
                    _simulationLoop?.Stop();
                    _animator.Stop();
                    _simulationLoop = Game1.StartCoroutine(SimulationLoop());
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
                List<string> validAnims = new List<string> { "ChainLightningUp", "ChainLightningDown", "ChainLightningLeft", "ChainLightningRight" };
                if (validAnims.Contains(_animator.CurrentAnimationName))
                {
                    if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                    {
                        _hitbox.SetEnabled(true);
                    }
                    else _hitbox.SetEnabled(false);
                }
                else _hitbox.SetEnabled(false);

                //check for hit
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_hitbox, _hitbox.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    foreach (var collider in colliders)
                    {
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

            if (State == PlayerActionState.Executing) Debug.Log($"playing animation ChainLightning{_direction}");
            _animator.Play($"ChainLightning{_direction}", SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAnimationFinished;

            bool hasPlayedSound = false;
            while (!_hasFinishedSwinging)
            {
                if (State == PlayerActionState.Executing)
                {
                    if (!hasPlayedSound)
                    {
                        if (_animator.CurrentAnimationName == $"ChainLightning{_direction}" && _hitboxActiveFrames.Contains(_animator.CurrentFrame))
                        {
                            hasPlayedSound = true;
                            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Big_lightning, .5f);
                            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._31_swoosh_sword_1);

                            var effectEntity = Scene.CreateEntity("lightning-effect");
                            effectEntity.SetPosition(Position + _hitbox.LocalOffset);
                            var effectComponent = effectEntity.AddComponent(new HitEffectComponent(HitEffects.Lightning1));
                            effectComponent.PlayAnimation();
                        }
                    }
                }

                yield return null;
            }

            Debug.Log("finished attack");
        }

        void OnAnimationFinished(string animation)
        {
            Debug.Log("animation finished");
            _animator.OnAnimationCompletedEvent -= OnAnimationFinished;
            _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
            _hasFinishedSwinging = true;
        }
    }
}
