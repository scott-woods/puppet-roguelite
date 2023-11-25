using Nez;
using Nez.Tweens;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class BossCutscene
    {
        BossRoom _bossRoom;

        public BossCutscene(BossRoom bossRoom)
        {
            _bossRoom = bossRoom;
        }

        public IEnumerator PlayScene()
        {
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            var cam = _bossRoom.Camera.Entity.GetComponent<DeadzoneFollowCamera>();
            cam.RemoveFollowTarget();

            var tween = TweenExt.TweenPositionTo(_bossRoom.Camera.Entity.Transform, _bossRoom.Boss.Entity.Position, 2.5f);
            tween.Start();
            yield return tween.WaitForCompletion();

            yield return Coroutine.WaitForSeconds(1);

            _bossRoom.Camera.ZoomIn(.2f);
            //Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Burp);
            yield return Coroutine.WaitForSeconds(1.5f);
            //Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.Galacta_knight, false, 2680);
            //yield return Game1.AudioManager.PlaySoundCoroutine(Nez.Content.Audio.Sounds.Burp);
            _bossRoom.Camera.ZoomOut(.2f);

            tween = TweenExt.TweenPositionTo(cam.Entity.Transform, PlayerController.Instance.Entity.Position, 2.68f);
            tween.Start();
            yield return tween.WaitForCompletion();

            cam.SetFollowTarget(PlayerController.Instance.Entity);

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);
            //_bossRoom.Boss.SetActive(true);

            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);
        }
    }
}
