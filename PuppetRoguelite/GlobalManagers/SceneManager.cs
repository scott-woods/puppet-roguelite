﻿using Microsoft.Xna.Framework;
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
        public SceneManagerState State = SceneManagerState.None;

        public string TargetEntranceId { get; set; }

        Scene _newScene;

        public void ChangeScene(Type targetSceneType, string targetEntranceId = "", Color? fadeToColor = null,
            float delayBeforeFadeInDuration = .2f, float fadeInDuration = .1f, float fadeOutDuration = .5f,
            EaseType fadeEaseType = EaseType.Linear, float delayBeforeTransitionStarts = .25f)
        {
            TargetEntranceId = targetEntranceId;

            var transition = new FadeTransition(() => _newScene = Activator.CreateInstance(targetSceneType) as Scene);
            transition.DelayBeforeFadeInDuration = delayBeforeFadeInDuration;
            transition.FadeToColor = fadeToColor == null ? Color.Black : fadeToColor.Value;
            transition.FadeInDuration = fadeInDuration;
            transition.FadeOutDuration = fadeOutDuration;
            transition.FadeEaseType = fadeEaseType;
            transition.OnTransitionCompleted += OnTransitionCompleted;
            transition.OnScreenObscured += OnScreenObscured;
            
            State = SceneManagerState.Transitioning;

            Emitter.Emit(SceneEvents.TransitionStarted);

            Game1.Schedule(delayBeforeTransitionStarts, timer =>
            {
                Game1.StartSceneTransition(transition);
            });
        }

        void OnTransitionCompleted()
        {
            State = SceneManagerState.ShowingScene;
            Emitter.Emit(SceneEvents.TransitionEnded);
        }

        void OnScreenObscured()
        {
            var transferEntity = TransferManager.Instance.GetEntityToTransfer();
            if (transferEntity != null)
            {
                transferEntity.DetachFromScene();
                transferEntity.SetEnabled(false);
            }

            Emitter.Emit(SceneEvents.ScreenObscured);
        }
    }

    public enum SceneEvents
    {
        TransitionStarted,
        ScreenObscured,
        TransitionEnded
    }

    public enum SceneManagerState
    {
        None,
        Transitioning,
        ShowingScene
    }
}
