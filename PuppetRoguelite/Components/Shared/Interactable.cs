using Nez;
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
            yield return Game1.StartCoroutine(_onInteracted());
            Active = true;
            _interactionCompletedCallback?.Invoke();
        }
    }
}
