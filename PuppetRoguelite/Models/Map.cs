using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class Map
    {
        public string Name { get; set; }
        public bool HasTop { get; set; }
        public bool HasBottom { get; set; }
        public bool HasLeft { get; set; }
        public bool HasRight { get; set; }
        public TmxMap TmxMap
        {
            get => Game1.Scene.Content.LoadTiledMap(Name);
        }

        public Map(string name, bool hasTop, bool hasBottom, bool hasLeft, bool hasRight)
        {
            Name = name;
            HasTop = hasTop;
            HasBottom = hasBottom;
            HasLeft = hasLeft;
            HasRight = hasRight;
        }

        public Map(string name)
        {
            Name = name;
        }
    }
}
