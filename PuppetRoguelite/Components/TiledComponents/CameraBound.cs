using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class CameraBound : TiledComponent
    {
        public CameraBoundType Type;
        public bool HasYDoorway = false;
        public bool HasXDoorway = false;

        public CameraBound(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
            if (tmxObject.Properties.TryGetValue("CameraBoundType", out var boundType))
            {
                Type = boundType switch
                {
                    "TopLeft" => CameraBoundType.TopLeft,
                    "BottomRight" => CameraBoundType.BottomRight,
                    _ => throw new Exception("Unrecognized camera bound type"),
                };
            }
            if (tmxObject.Properties.TryGetValue("HasYDoorway", out var yRes))
            {
                HasYDoorway = yRes.ToLower() == "true";
            }
            if (tmxObject.Properties.TryGetValue("HasXDoorway", out var xRes))
            {
                HasXDoorway = xRes.ToLower() == "true";
            }
        }
    }

    public enum CameraBoundType
    {
        TopLeft,
        BottomRight
    }
}
