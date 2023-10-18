﻿using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public abstract class Enemy : Component
    {
        public Entity MapEntity { get; set; }

        public Enemy(Entity mapEntity)
        {
            MapEntity = mapEntity;
        }
    }
}
