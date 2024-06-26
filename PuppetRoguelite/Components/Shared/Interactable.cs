﻿using Nez;
using Nez.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Interactable : Component
    {
        /// <summary>
        /// bool to determine if this can be interacted with currently
        /// </summary>
        public bool Active { get; set; }
        public int InteractionCount = 0;

        Func<IEnumerator> _onInteracted;
        Action _interactionCompletedCallback;

        public Interactable(Func<IEnumerator> onInteracted, bool active = true)
        {
            Active = active;
            _onInteracted = onInteracted;
        }

        public void Interact(Action interactionCompletedCallback)
        {
            if (Active)
            {
                if (_onInteracted != null)
                {
                    Active = false;
                    _interactionCompletedCallback = interactionCompletedCallback;
                    Game1.StartCoroutine(InteractionCoroutine());
                }
            }
        }

        IEnumerator InteractionCoroutine()
        {
            yield return _onInteracted();

            //wait a second before activating again, so we don't instantly re trigger
            Game1.Schedule(1f, timer =>
            {
                Active = true;
            });

            InteractionCount++;

            _interactionCompletedCallback?.Invoke();
        }
    }
}
