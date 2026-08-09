using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class BehaviorTree
    {
        private BehaviorNode root;

        public Blackboard blackboard;
        public bool HaveRoot=>root!=null;
        public BehaviorTree(BehaviorNode root,Blackboard _blackboard=null)
        {
            this.root=root;
            if(_blackboard==null)
            {
                blackboard = new Blackboard();
            }
            else
            {
                blackboard=_blackboard;
            }
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



