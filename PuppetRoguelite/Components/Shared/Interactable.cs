using Nez;
using Nez.Systems;
using System;
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
        public Emitter<InteractableEvents> Emitter = new Emitter<InteractableEvents>();

        public Interactable(bool active = true)
        {
            Active = active;
        }

        public void Interact()
        {
            if (Active)
            {
                Emitter.Emit(InteractableEvents.Interacted);
            }
        }
    }

    public enum InteractableEvents
    {
        Interacted
    }
}
