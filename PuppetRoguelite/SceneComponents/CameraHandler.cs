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
        public Vector2 TopLeft = new Vector2(int.MinValue, int.MinValue);
        public Vector2 BottomRight = new Vector2(int.MaxValue, int.MaxValue);
        public Vector2 RoomSize { get => BottomRight - TopLeft; }

        public void FormatCamera()
        {
            //get camera bounds
            var cameraBounds = Scene.FindComponentsOfType<CameraBound>();
            if (cameraBounds.Count > 0)
            {
                //get top left and bottom right bounds
                var topLeftBound = cameraBounds.FirstOrDefault(c => c.Type == CameraBoundType.TopLeft);
                if (topLeftBound != null) TopLeft = topLeftBound.Entity.Position;
                var bottomRightBound = cameraBounds.FirstOrDefault(c => c.Type == CameraBoundType.BottomRight);
                if (bottomRightBound != null) BottomRight = bottomRightBound.Entity.Position;

                //if both bounds are set
                if (topLeftBound != null && bottomRightBound != null)
                {
                    //handle y
                    if (RoomSize.Y < Scene.Camera.Bounds.Height)
                    {
                        //if room size is too small for camera, center the room in the camera view
                        var yDiff = Scene.Camera.Bounds.Height - RoomSize.Y;
                        int halfYDiff = (int)yDiff / 2;
                        TopLeft.Y -= halfYDiff;
                        BottomRight.Y += halfYDiff;

                        if (yDiff % 2 != 0)
                        {
                            BottomRight.Y += 1;
                        }
                    }

                    //handle x
                    if (RoomSize.X < Scene.Camera.Bounds.Width)
                    {
                        var xDiff = Scene.Camera.Bounds.Width - RoomSize.X;

                        //if both bounds don't have a door, or both do, center the map
                        if (topLeftBound.HasXDoorway == bottomRightBound.HasXDoorway)
                        {
                            int halfXDiff = (int)xDiff / 2;
                            TopLeft.X -= halfXDiff;
                            BottomRight.X += halfXDiff;

                            if (xDiff % 2 != 0)
                            {
                                BottomRight.X += 1;
                            }
                        }
                        else if (topLeftBound.HasXDoorway)
                        {
                            BottomRight.X += xDiff;
                        }
                        else if (bottomRightBound.HasXDoorway)
                        {
                            TopLeft.X -= xDiff;
                        }
                    }
                }
            }
        }
    }
}
