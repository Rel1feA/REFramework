using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{

    public class RepeatNode : DecoratorNode
    {
        private int counter;
        private int limit;
        public RepeatNode(int limit)
        {
            this.limit = limit;
        }

        protected override void OnInitialize()
        {
            counter = 0;
        }

        protected override E_BehaviorState OnUpdate()
        {
            while(true)
            {
                child.Tick();
                if(child.IsRunning)
                    return E_BehaviorState.Running;
                if(child.IsFailure)
                    return E_BehaviorState.Failure;
                if(++counter>=limit)
                    return E_BehaviorState.Success;
            }
        }
    }
}


