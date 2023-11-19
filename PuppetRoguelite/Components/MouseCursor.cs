﻿using Nez;
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
using PuppetRoguelite.Enums;

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

            Entity.SetScale(Game1.ResolutionScale / 2);
        }

        public void Update()
        {
            var mousePos = Entity.Scene.Camera.MouseToWorldPoint();
            Entity.Position = ResolutionHelper.GameToUiPoint(mousePos);
            //Entity.Position = Input.MousePosition * ResolutionHelper.ScaleFactor;
            //Entity.Position = Entity.Scene.Camera.ScreenToWorldPoint(Input.MousePosition);
            //Entity.Position = PlayerController.Instance.Entity.Position;
        }

        //public void Update()
        //{
        //    var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition);
        //    _baseElement.SetPosition(pos.X + _screenSpaceOffset.X, pos.Y + _screenSpaceOffset.Y);
        //}
    }
}
