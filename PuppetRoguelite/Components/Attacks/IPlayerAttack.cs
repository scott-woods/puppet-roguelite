﻿using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Attacks
{
    public interface IPlayerAttack
    {
        string Name { get; }
        int ApCost { get; }
        void Prepare();
        void Execute();
    }
}
