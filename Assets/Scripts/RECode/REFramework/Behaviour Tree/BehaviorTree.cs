using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class BehaviorTree
    {
        private BehaviorNode root;
        public bool HaveRoot=>root!=null;
        public BehaviorTree(BehaviorNode root)
        {
            this.root=root;
        }

        public void Tick()
        {
            root.Tick();
        }

        public void SetRoot(BehaviorNode root)
        {
            this.root=root;
        }
    }
}



