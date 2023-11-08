using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions
{
    public abstract class PlayerAction : Entity
    {
        public Action<PlayerAction, Vector2> ActionPrepFinishedHandler;
        public Action<PlayerAction> ActionPrepCanceledHandler;
        public Action<PlayerAction> ExecutionFinishedHandler;
        public PlayerActionState State = PlayerActionState.None;

        public PlayerAction(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler)
        {
            ActionPrepFinishedHandler = actionPrepFinishedHandler;
            ActionPrepCanceledHandler = actionPrepCanceledHandler;
            ExecutionFinishedHandler = executionFinishedHandler;
        }

        public virtual void Prepare()
        {
            State = PlayerActionState.Preparing;
        }
        public virtual void Execute()
        {
            State = PlayerActionState.Executing;
        }

        public override void Update()
        {
            base.Update();

            if (State == PlayerActionState.Preparing)
            {
                if (Input.IsKeyPressed(Keys.X))
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    State = PlayerActionState.None;
                    ActionPrepCanceledHandler?.Invoke(this);
                }
            }
        }

        public virtual void HandlePreparationFinished(Vector2 finalPosition)
        {
            ActionPrepFinishedHandler?.Invoke(this, finalPosition);
            State = PlayerActionState.None;
        }

        public virtual void HandleExecutionFinished()
        {
            ExecutionFinishedHandler?.Invoke(this);
            State = PlayerActionState.None;
        }
    }

    public enum PlayerActionCategory
    {
        Attack,
        Utility,
        Support
    }

    public enum PlayerActionState
    {
        None,
        Preparing,
        Executing
    }

    sealed class PlayerActionInfoAttribute : Attribute
    {
        public string Name { get; }
        public int ApCost { get; }
        public PlayerActionCategory Category { get; }

        public PlayerActionInfoAttribute(string name, int apCost, PlayerActionCategory category)
        {
            Name = name;
            ApCost = apCost;
            Category = category;
        }
    }

    public static class PlayerActionUtils
    {
        public static string GetName(Type actionType)
        {
            var attribute = (PlayerActionInfoAttribute)Attribute.GetCustomAttribute(actionType, typeof(PlayerActionInfoAttribute));
            return attribute?.Name;
        }

        public static int GetApCost(Type actionType)
        {
            var attribute = (PlayerActionInfoAttribute)Attribute.GetCustomAttribute(actionType, typeof(PlayerActionInfoAttribute));
            return attribute?.ApCost ?? 0;
        }

        public static PlayerActionCategory GetCategory(Type actionType)
        {
            var attribute = (PlayerActionInfoAttribute)Attribute.GetCustomAttribute(actionType, typeof(PlayerActionInfoAttribute));
            return attribute.Category;
        }
    }
}
