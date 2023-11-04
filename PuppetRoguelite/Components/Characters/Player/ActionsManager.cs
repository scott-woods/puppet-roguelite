using Nez;
using PuppetRoguelite.Components.PlayerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class ActionsManager : Component
    {
        public List<PlayerActionType> AttackActions = new List<PlayerActionType>();
        public List<PlayerActionType> UtilityActions = new List<PlayerActionType>();
        public List<PlayerActionType> SupportActions = new List<PlayerActionType>();

        public ActionsManager(List<PlayerActionType> attackActions, List<PlayerActionType> utilityActions,
            List<PlayerActionType> supportActions)
        {
            AttackActions = attackActions;
            UtilityActions = utilityActions;
            SupportActions = supportActions;
        }

        public void AddAction(Type type)
        {
            var category = PlayerActionUtils.GetCategory(type);
            switch (category)
            {
                case PlayerActionCategory.Attack:
                    AttackActions.Add(PlayerActionType.FromType(type));
                    break;
                case PlayerActionCategory.Utility:
                    UtilityActions.Add(PlayerActionType.FromType(type));
                    break;
                case PlayerActionCategory.Support:
                    SupportActions.Add(PlayerActionType.FromType(type));
                    break;
            }
        }

        public void RemoveAction(Type type)
        {
            var category = PlayerActionUtils.GetCategory(type);
            switch (category)
            {
                case PlayerActionCategory.Attack:
                    AttackActions.Remove(PlayerActionType.FromType(type));
                    break;
                case PlayerActionCategory.Utility:
                    UtilityActions.Remove(PlayerActionType.FromType(type));
                    break;
                case PlayerActionCategory.Support:
                    SupportActions.Remove(PlayerActionType.FromType(type));
                    break;
            }
        }
    }
}
