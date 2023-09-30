using Nez;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Entities;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    /// <summary>
    /// Handles the flow of combat in a scene
    /// </summary>
    public class CombatManager : SceneComponent
    {
        Stack<Component> _menuStack = new Stack<Component>();

        public CombatManager()
        {
            Game1.GameEventsEmitter.AddObserver(GameEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
        }

        public void OnTurnPhaseTriggered()
        {
            //freeze entities
            foreach(var entity in Scene.EntitiesOfType<PausableEntity>())
            {
                entity.TogglePaused();
            }
            var turnMenuEntity = Scene.CreateEntity("turn-menu");
            var attacksMenu = turnMenuEntity.AddComponent(new AttacksMenu(Player.Instance.Entity.Position, this));
            var actionSelector = turnMenuEntity.AddComponent(new ActionsSelector(Player.Instance.Entity.Position, this, attacksMenu));
            OpenMenu(actionSelector);
        }

        public void OpenMenu(Component component)
        {
            if (_menuStack.Count > 0)
            {
                _menuStack.Peek().SetEnabled(false);
            }
            _menuStack.Push(component);
            component.SetEnabled(true);
        }

        public void GoBack()
        {
            if (_menuStack.Count > 0)
            {
                var currentMenu = _menuStack.Pop();
                currentMenu.SetEnabled(false);
                if (_menuStack.Count > 0)
                {
                    _menuStack.Peek().SetEnabled(true);
                }
            }
        }
    }
}
