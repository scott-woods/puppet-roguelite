using Nez;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class Interactable : Component
    {
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
