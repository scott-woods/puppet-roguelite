using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class SongModel
    {
        public string Path { get; set; }
        public uint LoopTime { get; set; }

        public SongModel(string path, uint loopTime)
        {
            Path = path;
            LoopTime = loopTime;
        }
    }
}
