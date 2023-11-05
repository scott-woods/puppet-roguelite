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
        //protected bool _isSimulation;
        //protected bool _isPreparing = false;
        //public Vector2 InitialPosition;
        //public Vector2 FinalPosition;

        //public virtual void Prepare()
        //{
        //    _isPreparing = true;
        //    InitialPosition = Entity.Position;
        //    FinalPosition = Entity.Position;
        //}

        //public virtual void Execute(bool isSimulation = false)
        //{
        //    _isSimulation = isSimulation;
        //    var type = isSimulation ? PlayerActionEvents.SimActionExecuting : PlayerActionEvents.ActionExecuting;
        //    Emitters.PlayerActionEmitter.Emit(type, this);
        //}

        public Action<PlayerAction, Vector2> ActionPrepFinishedHandler;
        public Action<PlayerAction> ActionPrepCanceledHandler;
        public Action<PlayerAction> ExecutionFinishedHandler;

        public PlayerAction(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler)
        {
            ActionPrepFinishedHandler = actionPrepFinishedHandler;
            ActionPrepCanceledHandler = actionPrepCanceledHandler;
            ExecutionFinishedHandler = executionFinishedHandler;
        }

        public abstract void Prepare();
        public abstract void Execute();

        public override void Update()
        {
            base.Update();

            if (Input.IsKeyPressed(Keys.X))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                ActionPrepCanceledHandler?.Invoke(this);
            }
        }

        //public virtual void Update()
        //{
        //    if (_isPreparing)
        //    {
        //        if (Input.IsKeyPressed(Keys.E))
        //        {
        //            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select, .3f);
        //        }
        //        if (Input.IsKeyPressed(Keys.X))
        //        {
        //            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
        //            Core.Schedule(.1f, timer =>
        //            {
        //                _isPreparing = false;
        //                Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionPrepCanceled, this);
        //            });
        //        }
        //    }
        //}
    }

    public enum PlayerActionCategory
    {
        Attack,
        Utility,
        Support
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
