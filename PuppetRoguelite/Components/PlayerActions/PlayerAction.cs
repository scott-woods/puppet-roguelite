using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions
{
    public abstract class PlayerAction : Component, IPlayerAction, IUpdatable
    {
        protected bool _isSimulation;
        protected bool _isPreparing = false;
        public Vector2 InitialPosition;
        public Vector2 FinalPosition;

        public virtual void Prepare()
        {
            _isPreparing = true;
            InitialPosition = Entity.Position;
            FinalPosition = Entity.Position;
        }

        public virtual void Execute(bool isSimulation = false)
        {
            _isSimulation = isSimulation;
            var type = isSimulation ? PlayerActionEvents.SimActionExecuting : PlayerActionEvents.ActionExecuting;
            Emitters.PlayerActionEmitter.Emit(type, this);
        }

        public virtual void Update()
        {
            if (_isPreparing)
            {
                if (Input.IsKeyPressed(Keys.X))
                {
                    Core.Schedule(.1f, timer =>
                    {
                        _isPreparing = false;
                        Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionPrepCanceled, this);
                    });
                }
            }
        }
    }
}
