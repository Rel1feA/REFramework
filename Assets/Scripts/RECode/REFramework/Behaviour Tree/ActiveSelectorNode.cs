using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class ActiveSelector : SelectorNode
    {
        protected override E_BehaviorState OnUpdate()
        {
            var prev = currentChild;
            base.OnInitialize();
            var res=base.OnUpdate();
            if (prev != null && currentChild !=prev)
            {
                prev.Value.Abort();
            }
            return res;
        }
    }
}



