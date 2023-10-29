using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Characters.HeartHoarder;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Spitter;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nez.Content.Textures;

namespace PuppetRoguelite.Scenes
{
    public class BossRoom : BaseScene
    {
        public const string MapString = Nez.Content.Tiled.Tilemaps.Boss_room;

        //scene components
        public TiledObjectHandler TiledObjectHandler;
        public CombatManager CombatManager;

        //entities
        Entity _playerEntity;
        Entity _mapEntity;

        //components
        TiledMapRenderer _mapRenderer;
        //TiledMapComponent _mapComponent;

        public BossSpawnPoint BossSpawnPoint;
        public HeartHoarder Boss;

        public List<Enemy> FodderEnemies = new List<Enemy>();
        public ITimer EnemySpawnTimer;
        bool _isEnemySpawnTimerActive = false;
        float _enemySpawnTime = 0;
        float _enemySpawnCompletionTime = 10f;

        public override void Initialize()
        {
            base.Initialize();

            //CREATE MAP

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Boss_room);
            var mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
            mapRenderer.RenderLayer = 10;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

            //grid graph
            var gridGraphManager = _mapEntity.AddComponent(new GridGraphManager(mapRenderer));

            //tiled obj handler
            var tiledObjectHandler = _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));

            //ui
            Camera.Entity.AddComponent(new CombatUI());

            //add player
            _playerEntity = new Entity("player");
            AddEntity(_playerEntity);
            var player = _playerEntity.AddComponent(new PlayerController());
            _playerEntity.SetEnabled(false);

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //add combat manager
            CombatManager = AddSceneComponent(new CombatManager());

            //game state
            AddSceneComponent(new GameStateManager());

            var mouseEntity = CreateEntity("mouse");
            mouseEntity.AddComponent(new MouseCursor());
        }

        public override void Begin()
        {
            base.Begin();

            BossSpawnPoint = FindComponentOfType<BossSpawnPoint>();

            //connect to trigger
            var trigger = FindComponentOfType<BossTrigger>();
            trigger.Emitter.AddObserver(TriggerEventTypes.Triggered, OnTriggered);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        public override void OnStart()
        {
            base.OnStart();

            //spawn player
            var spawn = FindComponentOfType<PlayerSpawnPoint>();
            _playerEntity.SetPosition(spawn.Entity.Position.X, spawn.Entity.Position.Y);
            _playerEntity.SetEnabled(true);

            //spawn boss
            //var bossSpawn = FindComponentOfType<BossSpawnPoint>();
            //var bossEntity = AddEntity(new PausableEntity("boss"));
            //bossEntity.SetPosition(bossSpawn.Entity.Position);
            //Boss = bossEntity.AddComponent(new HeartHoarder(_mapEntity));
            //Boss.SetEnabled(false);
        }

        public override void Update()
        {
            base.Update();

            if (_isEnemySpawnTimerActive)
            {
                _enemySpawnTime += Time.DeltaTime;
                if (_enemySpawnTime >= _enemySpawnCompletionTime)
                {
                    Game1.StartCoroutine(SpawnFodderEnemies());
                    _isEnemySpawnTimerActive = false;
                    _enemySpawnTime = 0;
                }
            }
        }

        void OnTriggered()
        {
            Game1.StartCoroutine(StartBattle());
            //var cutscene = new BossCutscene(this);
            //Game1.StartCoroutine(cutscene.PlayScene());
        }

        IEnumerator StartBattle()
        {
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //disable ui
            var ui = Camera.Entity.GetComponent<CombatUI>();
            ui.SetEnabled(false);

            //lock gates
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Gate_close);
            var gates = FindComponentsOfType<Gate>().Where(g => g.MapEntity == _mapEntity).ToList();
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            //wait
            yield return Coroutine.WaitForSeconds(1f);

            //start music
            Game1.AudioManager.PlayMusic(Music.HaveAHeart, true);

            //pan camera to boss spawn position
            var cam = Camera.Entity.GetComponent<DeadzoneFollowCamera>();
            cam.RemoveFollowTarget();
            var tween = TweenExt.TweenPositionTo(Camera.Entity.Transform, BossSpawnPoint.Entity.Position + new Vector2(0, 32), 1.3f);
            tween.Start();
            yield return tween.WaitForCompletion();

            //spawn boss
            var bossEntity = AddEntity(new PausableEntity("boss"));
            bossEntity.SetPosition(BossSpawnPoint.Entity.Position);
            Boss = bossEntity.AddComponent(new HeartHoarder(_mapEntity));
            CombatManager.AddEnemy(Boss);
            Boss.HealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnBossDied);

            //play appearance animation (should be about 1.2 seconds)
            yield return Boss.PlayAppearanceAnimation();

            //turn on boss's BT
            Boss.SetBehaviorTreeEnabled(true);

            //linger on boss for a second
            yield return Coroutine.WaitForSeconds(1f);

            //spawn fodder enemies (should be about 1 second)
            yield return SpawnFodderEnemies();

            //linger again
            yield return Coroutine.WaitForSeconds(1f);

            //tween camera back to player
            tween = TweenExt.TweenPositionTo(cam.Entity.Transform, PlayerController.Instance.Entity.Position, 2f);
            tween.SetEaseType(Nez.Tweens.EaseType.CubicIn);
            tween.Start();
            yield return tween.WaitForCompletion();

            //set cam follow target to player again
            cam.SetFollowTarget(PlayerController.Instance.Entity);

            //emit cutscene ended signal
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //re enable ui
            ui.SetEnabled(true);

            //start encounter
            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);
        }

        IEnumerator EndBattle()
        {
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //pan camera to boss position
            var cam = Camera.Entity.GetComponent<DeadzoneFollowCamera>();
            cam.RemoveFollowTarget();
            var tween = TweenExt.TweenPositionTo(Camera.Entity.Transform, Boss.Entity.Position + new Vector2(0, 32), .25f);
            tween.SetEaseType(Nez.Tweens.EaseType.Linear);
            tween.Start();
            yield return tween.WaitForCompletion();

            //wait until boss is gone
            while (Boss.Entity != null)
            {
                yield return null;
            }

            //tween camera back to player
            tween = TweenExt.TweenPositionTo(cam.Entity.Transform, PlayerController.Instance.Entity.Position, .25f);
            tween.SetEaseType(Nez.Tweens.EaseType.Linear);
            tween.Start();
            yield return tween.WaitForCompletion();

            //set cam follow target to player again
            cam.SetFollowTarget(PlayerController.Instance.Entity);

            //emit cutscene ended signal
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);
        }

        IEnumerator SpawnFodderEnemies()
        {
            var enemySpawnPoints = FindComponentsOfType<EnemySpawnPoint>();
            foreach (var spawnPoint in enemySpawnPoints)
            {
                var typeProp = spawnPoint.TmxObject.Properties["type"];
                Type enemyType;
                switch (typeProp)
                {
                    case "spitter":
                        enemyType = typeof(Spitter);
                        break;
                    case "chainBot":
                        enemyType = typeof(ChainBot);
                        break;
                    default:
                        enemyType = typeof(Spitter);
                        break;
                }
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Enemy_spawn);
                var enemy = spawnPoint.SpawnEnemy(enemyType);
                if (enemy.Entity.TryGetComponent<HealthComponent>(out var healthComponent))
                {
                    healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnEnemyDied);
                }
                CombatManager.AddEnemy(enemy);
                FodderEnemies.Add(enemy);

                yield return Coroutine.WaitForSeconds(.2f);
            }
        }

        void OnEnemyDied(HealthComponent hc)
        {
            var enemy = hc.Entity.GetComponent<Enemy>();
            FodderEnemies.Remove(enemy);

            if (FodderEnemies.Count == 0)
            {
                _isEnemySpawnTimerActive = true;
            }
        }

        void OnBossDied(HealthComponent hc)
        {
            Game1.AudioManager.StopMusic();
            foreach(var enemy in FodderEnemies)
            {
                enemy.Entity.Destroy();
            }

            FodderEnemies.Clear();

            CombatManager.EndCombat();

            Game1.StartCoroutine(EndBattle());
        }

        void OnTurnPhaseTriggered()
        {
            _isEnemySpawnTimerActive = false;
        }

        void OnDodgePhaseStarted()
        {
            if (FodderEnemies.Count == 0)
            {
                _isEnemySpawnTimerActive = true;
            }
        }
    }
}
