using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class MonitorNode : ParallelNode
    {
        public MonitorNode(E_Policy mSuccessPolicy,E_Policy mFailurePolicy):base(mSuccessPolicy,mFailurePolicy)
        {

        }

        public void AddCondition(BehaviorNode condition)
        {
            children.AddFirst(condition);
        }
        public void AddAction(BehaviorNode action)
        {
            children.AddLast(action);
        }
    }
}

