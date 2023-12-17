using Nez;
using PuppetRoguelite.Cutscenes;
using PuppetRoguelite.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class EventManager : GlobalManager
    {
        Dictionary<string, Func<IEnumerator>> _eventDictionary;

        public EventManager()
        {
            _eventDictionary = new Dictionary<string, Func<IEnumerator>>()
            {
                { "DashTutorial", TutorialCutscenes.DashTutorial },
                { "AttackTutorial", TutorialCutscenes.AttackTutorial },
                { "CombatTutorial", TutorialCutscenes.CombatTutorial },
                { "TutorialFinal", TutorialCutscenes.TutorialFinal },
                { "TutorialShelf", TutorialCutscenes.TutorialShelf },
                { "HubShelf", InspectableEvents.HubShelf }
            };
        }

        public IEnumerator PlayEvent(string eventName)
        {
            if (_eventDictionary.TryGetValue(eventName, out Func<IEnumerator> eventFunc))
            {
                yield return eventFunc();
            }
        }
    }
}
