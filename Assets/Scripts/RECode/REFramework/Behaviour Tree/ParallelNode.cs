using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class ParallelNode: CompositeNode
    {
        public enum E_Policy
        {
            RequireOne,RequireAll
        }

        protected E_Policy mSuccessPolicy;//成功的标准
        protected E_Policy mFailurePolicy;//失败的标准

        public ParallelNode(E_Policy mSuccessPolicy, E_Policy mFailurePolicy)
        {
            this.mSuccessPolicy = mSuccessPolicy;
            this.mFailurePolicy = mFailurePolicy;
        }

        protected override E_BehaviorState OnUpdate()
        {
            int successCount = 0,failureCount=0;
            var b = children.First;
            var size=children.Count;
            for(int i = 0; i < size; i++)
            {
                var node=b.Value;
                if(!node.IsTerminated)node.Tick();
                b=b.Next;
                if(node.IsSuccess)
                {
                    successCount++;
                    if(mSuccessPolicy==E_Policy.RequireOne)
                        return E_BehaviorState.Success;
                }
                if(node.IsFailure)
                {
                    failureCount++;
                    if (mFailurePolicy == E_Policy.RequireOne)
                        return E_BehaviorState.Failure;
                }
            }
            if(mFailurePolicy==E_Policy.RequireAll&&failureCount==size)
                return E_BehaviorState.Failure;
            if(mSuccessPolicy==E_Policy.RequireAll&&successCount==size)
                return E_BehaviorState.Success;
            return E_BehaviorState.Running;
        }

        protected override void OnTerminate()
        {
            foreach(var node in children)
            {
                if(node.IsRunning)node.Abort();
            }
        }
    }
}


