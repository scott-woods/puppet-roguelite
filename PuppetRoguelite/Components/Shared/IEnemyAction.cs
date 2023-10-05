using Nez;
using Nez.AI.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public interface IEnemyAction
    {
        bool IsCompleted { get; }
        BehaviorTree<Enemy> GetBehaviorTree();
        void Start();
        void Reset();
    }
}
