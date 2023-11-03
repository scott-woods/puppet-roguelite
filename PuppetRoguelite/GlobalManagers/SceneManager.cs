using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.Tweens;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class SceneManager : GlobalManager
    {
        public Emitter<SceneEvents> Emitter = new Emitter<SceneEvents>();

        public string TargetEntranceId { get; set; }

        Scene _newScene;

        public void ChangeScene(Type targetSceneType, string targetEntranceId = "", Color? fadeToColor = null,
            float delayBeforeFadeInDuration = .5f, float fadeInDuration = .8f, float fadeOutDuration = .8f,
            EaseType fadeEaseType = EaseType.Linear)
        {
            var transferEntity = TransferManager.Instance.GetEntityToTransfer();
            if (transferEntity != null)
            {
                transferEntity.DetachFromScene();
                transferEntity.SetEnabled(false);
            }

            TargetEntranceId = targetEntranceId;

            var transition = new FadeTransition(() => _newScene = Activator.CreateInstance(targetSceneType) as Scene);
            transition.DelayBeforeFadeInDuration = delayBeforeFadeInDuration;
            transition.FadeToColor = fadeToColor == null ? Color.Black : fadeToColor.Value;
            transition.FadeInDuration = fadeInDuration;
            transition.FadeOutDuration = fadeOutDuration;
            transition.FadeEaseType = fadeEaseType;
            transition.OnTransitionCompleted += OnTransitionCompleted;

            Game1.StartSceneTransition(transition);

            Emitter.Emit(SceneEvents.TransitionStarted);
        }

        void OnTransitionCompleted()
        {
            Emitter.Emit(SceneEvents.TransitionEnded);
        }
    }

    public enum SceneEvents
    {
        TransitionStarted,
        TransitionEnded
    }
}
