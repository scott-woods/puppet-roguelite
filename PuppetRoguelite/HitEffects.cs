using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class HitEffects
    {
        public static HitEffect Lightning1 { get => new HitEffect(Nez.Content.Textures.Effects.Electric_hit_1, 82, 65); }
        public static HitEffect Lightning2 { get => new HitEffect(Nez.Content.Textures.Effects.Electric_hit_2, 82, 65); }
    }
}
