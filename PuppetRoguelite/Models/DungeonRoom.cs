﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.TiledComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class DungeonRoom : Component
    {
        public Map Map;
        public Vector2 Position;
        public List<DungeonDoorway> Doorways = new List<DungeonDoorway>();
        public List<EnemySpawnTrigger> EnemySpawnTriggers = new List<EnemySpawnTrigger>();
        public DungeonLeaf DungeonLeaf;

        public DungeonRoom(Map map, Vector2 position, DungeonLeaf dungeonLeaf)
        {
            Map = map;
            Position = position;
            DungeonLeaf = dungeonLeaf;
        }
    }
}
