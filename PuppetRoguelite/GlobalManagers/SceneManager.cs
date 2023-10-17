﻿using Nez;
using Nez.Systems;
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

        public Scene ActiveScene { get; set; }

        public void ChangeScene(Type sceneType)
        {
            var transition = Game1.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(sceneType) as Scene));
            transition.OnTransitionCompleted += OnTransitionCompleted;

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