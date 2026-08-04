using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class Filter : SequenceNode
    {
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


