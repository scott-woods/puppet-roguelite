using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public static class Music
    {
        public static Song HaveAHeart = new Song(Nez.Content.Audio.Music.Have_a_heart, 97500);
        public static Song Babbulon = new Song(Nez.Content.Audio.Music.Babbulon_double, 125902);
        public static Song TheBay = new Song(Nez.Content.Audio.Music.The_bay, 0);
    }
}
