using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class DialogueLine
    {
        public string Text { get; set; }
        public Sprite Portrait { get; set; }

        public DialogueLine(string text, Sprite portrait = null)
        {
            Text = text;
            Portrait = portrait;
        }
    }
}
