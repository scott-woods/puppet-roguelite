using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetRoguelite.Models;

namespace PuppetRoguelite.StaticData
{
    public static class Songs
    {
        public static SongModel HaveAHeart = new SongModel(Nez.Content.Audio.Music.Have_a_heart, 97500);
        public static SongModel Babbulon = new SongModel(Nez.Content.Audio.Music.Babbulon_double, 125902);
        public static SongModel TheBay = new SongModel(Nez.Content.Audio.Music.The_bay, 0);
        public static SongModel BabyMode = new SongModel(Nez.Content.Audio.Music.Baby_mode, 0);
        public static SongModel GalactaKnight = new SongModel(Nez.Content.Audio.Music.Galacta_knight, 0);
        public static SongModel Menu = new SongModel(Nez.Content.Audio.Music.Threadlock_menu, 0);
    }
}
