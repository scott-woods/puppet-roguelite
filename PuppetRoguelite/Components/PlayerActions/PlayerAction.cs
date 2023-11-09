using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;
using Nez.UI;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
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
        public Emitter<PlayerActionEvent, PlayerAction> Emitter = new Emitter<PlayerActionEvent, PlayerAction>();
        public bool ShouldDestroySelf;
        public PlayerActionState State = PlayerActionState.None;
        public Vector2 InitialPosition, FinalPosition;

        public PlayerAction(bool shouldDestroySelf = false)
        {
            ShouldDestroySelf = shouldDestroySelf;
        }

        public virtual void Prepare()
        {
            InitialPosition = Position;
            FinalPosition = Position;
            Emitter.Emit(PlayerActionEvent.PrepStarted, this);
            State = PlayerActionState.Preparing;
        }
        public virtual void Execute()
        {
            Emitter.Emit(PlayerActionEvent.ExecutionStarted, this);
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
                    HandlePreparationCanceled();
                }
            }
            else if (State == PlayerActionState.Executing)
            {
                PlayerController.Instance.Entity.SetPosition(Position);
            }
        }

        public virtual void HandlePreparationFinished(Vector2 finalPosition)
        {
            State = PlayerActionState.None;
            PlayerController.Instance.ActionPointComponent.DecrementActionPoints(PlayerActionUtils.GetApCost(GetType()));
            Emitter.Emit(PlayerActionEvent.PrepFinished, this);
        }

        public virtual void HandlePreparationCanceled()
        {
            State = PlayerActionState.None;
            Emitter.Emit(PlayerActionEvent.PrepCanceled, this);
        }

        public virtual void HandleExecutionFinished()
        {
            State = PlayerActionState.None;
            Emitter.Emit(PlayerActionEvent.ExecutionFinished, this);
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

    public enum PlayerActionEvent
    {
        PrepCanceled,
        PrepStarted,
        PrepFinished,
        ExecutionStarted,
        ExecutionFinished,
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
