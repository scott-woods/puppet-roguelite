﻿using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Tools
{
    /// <summary>
    /// Helper to convert positions when working with two render targets at different resolutions
    /// </summary>
    public static class ResolutionHelper
    {
        public static Vector2 ScaleFactor { get; set; }

        public static void SetScaleFactor(Vector2 scaleFactor)
        {
            ScaleFactor = scaleFactor;
        }

        public static Vector2 GameToUiPoint(Entity entity, Vector2 gamePoint)
        {
            var screenPos = entity.Scene.Camera.WorldToScreenPoint(gamePoint);
            return screenPos * ScaleFactor;
        }

        public static Vector2 UiToGamePoint(Entity entity, Vector2 uiPoint)
        {
            var worldPos = entity.Scene.Camera.ScreenToWorldPoint(uiPoint);
            return worldPos / ScaleFactor;
        }
    }
}
