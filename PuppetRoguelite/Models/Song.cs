using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class Song
    {
        public string Path { get; set; }
        public uint LoopTime { get; set; }

        public Song(string path, uint loopTime)
        {
            Path = path;
            LoopTime = loopTime;
        }
    }
}
