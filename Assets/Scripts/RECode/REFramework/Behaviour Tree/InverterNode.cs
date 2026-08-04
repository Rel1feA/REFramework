using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RECode.REFramework
{
    public class InverterNode : DecoratorNode
    {
        protected override E_BehaviorState OnUpdate()
        {
            child.Tick();
            if(child.IsFailure)return E_BehaviorState.Success;
            if(child.IsSuccess)return E_BehaviorState.Failure;
            return E_BehaviorState.Running;
        }
    }
}


