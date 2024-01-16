using Nez;
using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.StaticData
{
    public class Maps
    {
        public static List<Map> DungeonPrisonMaps = new List<Map>()
        {
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_b_1, false, true, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_b_2, false, true, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_bl_1, false, true, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_blr_1, false, true, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_br_1, false, true, false, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_l_1, false, false, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_lr_1, false, false, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_r_1, false, false, false, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_t_1, true, false, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tb_1, true, true, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tbl_1, true, true, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tblr_1, true, true, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tbr_1, true, true, false, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tl_1, true, false, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tlr_1, true, false, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tr_1, true, false, false, true),
        };

        public static List<Map> ForgeMaps = new List<Map>()
        {
            new Map(Nez.Content.Tiled.Tilemaps.Forge.Forge_simple)
        };

        public static Map ForgePreBoss = new Map(Nez.Content.Tiled.Tilemaps.Forge.Forge_pre_boss);
    }
}
