﻿using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Tools
{
    public class TransferManager
    {
        public static readonly TransferManager Instance = new TransferManager();

        Entity _entityToTransfer;

        private TransferManager() { }

        public void SetEntityToTransfer(Entity entity)
        {
            _entityToTransfer = entity;
        }

        public Entity GetEntityToTransfer()
        {
            var entity = _entityToTransfer;
            return entity;
        }

        public void ClearEntityToTransfer()
        {
            _entityToTransfer = null;
        }
    }
}
