using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.StaticData
{
    public class HitEffects
    {
        public static HitEffect Hit1 { get => new HitEffect(Nez.Content.Textures.Effects.Hit1, 82, 65); }
        public static HitEffect Hit2 { get => new HitEffect(Nez.Content.Textures.Effects.Hit2, 82, 65); }
        public static HitEffect Hit3 { get => new HitEffect(Nez.Content.Textures.Effects.Hit3, 82, 65); }
        public static HitEffect Hit4 { get => new HitEffect(Nez.Content.Textures.Effects.Hit4, 82, 65); }
        public static HitEffect Lightning1 { get => new HitEffect(Nez.Content.Textures.Effects.Electric_hit_1, 82, 65); }
        public static HitEffect Lightning2 { get => new HitEffect(Nez.Content.Textures.Effects.Electric_hit_2, 82, 65); }
    }
}
