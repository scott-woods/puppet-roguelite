using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.TiledComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class CameraHandler : SceneComponent
    {
        public float Top, Bottom, Left, Right;
        public RectangleF RoomBounds;
        public bool IsFormatted = false;

        public void FormatCamera()
        {
            var cameraBounds = Scene.FindComponentsOfType<CameraBound>();
            if (cameraBounds.Count > 0)
            {
                IsFormatted = true;
                foreach (var cameraBound in cameraBounds)
                {
                    Top = cameraBound.Entity.Position.Y < Top ? cameraBound.Entity.Position.Y : Top;
                    Bottom = cameraBound.Entity.Position.Y > Bottom ? cameraBound.Entity.Position.Y : Bottom;
                    Left = cameraBound.Entity.Position.X < Left ? cameraBound.Entity.Position.X : Left;
                    Right = cameraBound.Entity.Position.X > Right ? cameraBound.Entity.Position.X : Right;
                }

                RoomBounds = new RectangleF(Left, Top, Right - Left, Bottom - Top);

                var scaleX = Scene.Camera.Bounds.Width / RoomBounds.Width;
                var scaleY = Scene.Camera.Bounds.Height / RoomBounds.Height;

                var targetZoom = Math.Min(scaleX, scaleY);
                targetZoom = (int)Math.Floor(targetZoom);

                Scene.Camera.SetMaximumZoom(targetZoom);
                Scene.Camera.SetZoom(1);
            }
            else IsFormatted = false;
        }

        public override void Update()
        {
            base.Update();

            //if (IsFormatted)
            //{
            //    if (Scene.Camera.Bounds.Top < Top)
            //    {
            //        Scene.Camera.Position += new Vector2(0, Top - Scene.Camera.Bounds.Top);
            //    }
            //    if (Scene.Camera.Bounds.Bottom > Bottom)
            //    {
            //        Scene.Camera.Position += new Vector2(0, Bottom - Scene.Camera.Bounds.Bottom);
            //    }
            //    if (Scene.Camera.Bounds.Left < Left)
            //    {
            //        Scene.Camera.Position += new Vector2(Left - Scene.Camera.Bounds.Left, 0);
            //    }
            //    if (Scene.Camera.Bounds.Right > Right)
            //    {
            //        Scene.Camera.Position += new Vector2(Right - Scene.Camera.Bounds.Right, 0);
            //    }
            //}
        }
    }
}
