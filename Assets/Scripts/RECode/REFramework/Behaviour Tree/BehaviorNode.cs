namespace RECode.REFramework
{
    public enum E_BehaviorState
    {
        Failure,//失败
        Success,//成功
        Running,//运行中
        Aborted,//中断
        Invalid//无效
    }

    public abstract class BehaviorNode
    {
        public bool IsTerminated => IsSuccess || IsFailure;//是否运行结束
        public bool IsSuccess => state == E_BehaviorState.Success;//是否成功
        public bool IsFailure => state == E_BehaviorState.Failure;//是否失败
        public bool IsRunning => state == E_BehaviorState.Running;//是否正在运行

        protected E_BehaviorState state;
        protected Blackboard blackboard;

        public BehaviorNode(Blackboard blackboard)
        {
            state = E_BehaviorState.Invalid;
            this.blackboard = blackboard;
        }

        protected virtual void OnInitialize() { }
        protected abstract E_BehaviorState OnUpdate();
        protected virtual void OnTerminate() { }

        public E_BehaviorState Tick()
        {
            if (!IsRunning) OnInitialize();
            state = OnUpdate();
            if (!IsRunning) OnTerminate();
            return state;
        }
        public virtual void AddChild(BehaviorNode child) { }

        public void Reset()
        {
            state = E_BehaviorState.Invalid;
        }

        public void Abort()
        {
            OnTerminate();
            state = E_BehaviorState.Aborted;
        }
    }
}


