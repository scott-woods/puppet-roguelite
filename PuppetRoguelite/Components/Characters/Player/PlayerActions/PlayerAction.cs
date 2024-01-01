using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;
using Nez.UI;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.StaticData;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions
{
    public abstract class PlayerAction : Entity
    {
        public Emitter<PlayerActionEvent, PlayerAction> Emitter = new Emitter<PlayerActionEvent, PlayerAction>();
        public bool ShouldDestroySelf;
        public PlayerActionState State = PlayerActionState.None;
        public Vector2 InitialPosition, FinalPosition;

        bool _isTutorialMode = false;

        public PlayerAction(bool shouldDestroySelf = false)
        {
            ShouldDestroySelf = shouldDestroySelf;
        }

        public virtual void Prepare()
        {
            Log.Information($"Preparing Action: {this}");

            InitialPosition = Position;
            FinalPosition = Position;
            Emitter.Emit(PlayerActionEvent.PrepStarted, this);
            State = PlayerActionState.Preparing;
        }
        public virtual void Execute()
        {
            Log.Information($"Executing Action: {this}");

            Emitter.Emit(PlayerActionEvent.ExecutionStarted, this);
            State = PlayerActionState.Executing;
        }

        public override void Update()
        {
            base.Update();

            if (State == PlayerActionState.Preparing)
            {
                if (Controls.Instance.Cancel.IsPressed && !_isTutorialMode)
                {
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
            Log.Information($"Finished Preparing Action: {this}");

            State = PlayerActionState.None;
            PlayerController.Instance.ActionPointComponent.DecrementActionPoints(PlayerActionUtils.GetApCost(GetType()));
            Emitter.Emit(PlayerActionEvent.PrepFinished, this);
        }

        public virtual void HandlePreparationCanceled()
        {
            Log.Information($"Canceled Preparing Action: {this}");

            Reset();

            Game1.AudioManager.PlaySound(Content.Audio.Sounds._021_Decline_01);
            State = PlayerActionState.None;
            Emitter.Emit(PlayerActionEvent.PrepCanceled, this);
        }

        public virtual void HandleExecutionFinished()
        {
            Log.Information($"Finished Executing Action: {this}");

            Reset();
            State = PlayerActionState.None;
            Emitter.Emit(PlayerActionEvent.ExecutionFinished, this);
        }

        public virtual void SetTutorialModeEnabled(bool enabled)
        {
            _isTutorialMode = enabled;
        }

        public abstract void Reset();
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
        public string Description { get; }

        public PlayerActionInfoAttribute(string name, int apCost, PlayerActionCategory category, string description)
        {
            Name = name;
            ApCost = apCost;
            Category = category;
            Description = description;
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

        public static string GetDescription(Type actionType)
        {
            var attribute = (PlayerActionInfoAttribute)Attribute.GetCustomAttribute(actionType, typeof(PlayerActionInfoAttribute));
            return attribute?.Description;
        }
    }
}
