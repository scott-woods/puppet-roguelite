using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetRoguelite.Components.Characters.Player;
using Microsoft.Xna.Framework.Input;
using PuppetRoguelite.Tools;
using Nez.UI;
using Microsoft.Xna.Framework;
using PuppetRoguelite.StaticData;

namespace PuppetRoguelite.Components
{
    public class MouseCursor : Component, IUpdatable
    {
        public SpriteRenderer SpriteRenderer;

        public override void Initialize()
        {
            base.Initialize();

            var texture = Game1.Content.LoadTexture(Nez.Content.Textures.UI.Crosshair038);
            SpriteRenderer = Entity.AddComponent(new SpriteRenderer(texture));

            SpriteRenderer.SetRenderLayer((int)RenderLayers.Cursor);

            //Entity.SetScale(Game1.ResolutionScale / 2);
            var scale = Screen.Size / Game1.UIResolution.ToVector2();
            Entity.SetScale(scale);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Game1.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Game1.Emitter.RemoveObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
        }

        public void Update()
        {
            Entity.Position = Input.RawMousePosition.ToVector2();
        }

        void OnGraphicsDeviceReset()
        {
            var scale = Screen.Size / Game1.UIResolution.ToVector2();
            Entity.SetScale(scale);
        }

        //public void Update()
        //{
        //    var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition);
        //    _baseElement.SetPosition(pos.X + _screenSpaceOffset.X, pos.Y + _screenSpaceOffset.Y);
        //}
    }
}
