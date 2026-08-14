using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RECode.REFramework
{
    public interface IEventInfo
    {
        Delegate[] GetDelegates();
    }

    public class EventInfo<T> : IEventInfo
    {
        public UnityAction<T> actions;

        public EventInfo(UnityAction<T> _action)
        {
            actions += _action;
        }

        public Delegate[] GetDelegates()
        {
            return actions.GetInvocationList();
        }
    }

    public class FuncInfo<T> : IEventInfo
    {
        public Func<T> func;

        public FuncInfo(Func<T> _func)
        {
            func += _func;
        }

        public Delegate[] GetDelegates()
        {
            return func.GetInvocationList();
        }
    }

    public class EventInfo : IEventInfo
    {
        public UnityAction actions;

        public EventInfo(UnityAction _action)
        {
            actions += _action;
        }

        public Delegate[] GetDelegates()
        {
            return actions.GetInvocationList();
        }

    }
    public class EventCenter : NormalSingleton<EventCenter>
    {
        private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();
        public Dictionary<string,IEventInfo> EventDic {  get { return eventDic; } }

        public void AddListener<T>(string name, UnityAction<T> action)
        {
            if (eventDic.TryGetValue(name,out IEventInfo info))
            {
                if(info is EventInfo<T> eventInfo)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    Debug.LogError($"事件 {name} 类型不匹配，期望有参数事件，但添加的是无参事件");
                }
            }
            else
            {
                eventDic.Add(name, new EventInfo<T>(action));
            }
        }

        public void AddListener(string name, UnityAction action)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info))
            {
                if (info is EventInfo eventInfo)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    Debug.LogError($"事件 {name} 类型不匹配，期望无参数事件，但添加的是有参事件");
                }
            }
            else
            {
                eventDic.Add(name, new EventInfo(action));
            }
        }

        public void RemoveListener<T>(string name, UnityAction<T> action)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info))
            {
                if (info is EventInfo<T> eventInfo)
                {
                    eventInfo.actions -= action;
                    if(eventInfo.actions==null)
                    {
                        eventDic.Remove(name);
                    }
                }
                else
                {
                    Debug.LogError($"事件 {name} 类型不匹配，期望有参数事件，但移除的是无参事件");
                }
            }
        }

        public void RemoveListener(string name, UnityAction action)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info))
            {
                if (info is EventInfo eventInfo)
                {
                    eventInfo.actions -= action;
                    if (eventInfo.actions == null)
                    {
                        eventDic.Remove(name);
                    }
                }
                else
                {
                    Debug.LogError($"事件 {name} 类型不匹配，期望无参数事件，但移除的是有参事件");
                }
            }
        }

        public void AddFuncListener<T>(string name, Func<T> func)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info))
            {
                if (info is FuncInfo<T> funcInfo)
                    funcInfo.func+= func;
                else
                    Debug.LogError($"委托 {name} 类型不匹配，期望条件事件");
            }
            else
            {
                eventDic.Add(name, new FuncInfo<T>(func));
            }
        }

        public void RemoveFuncListener<T>(string name, Func<T> func)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info) && info is FuncInfo<T> funcInfo)
            {
                funcInfo.func -= func;
                if (funcInfo.func == null)
                    eventDic.Remove(name);
            }
        }

        public T FuncTrigger<T>(string name)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info) && info is FuncInfo<T> funcInfo)
            {
                return funcInfo.func.Invoke();
            }
            return default(T);
        }

        public void EventTrigger<T>(string name, T arg)
        {
            if (eventDic.TryGetValue(name, out IEventInfo info) && info is EventInfo<T> eventInfo)
            {
                eventInfo.actions?.Invoke(arg);
            }
        }

        public void EventTrigger(string name)
        {
            if(eventDic.TryGetValue(name,out IEventInfo info)&&info is EventInfo eventInfo)
            {
                eventInfo.actions?.Invoke();
            }
        }

        public void Clear()
        {
            eventDic.Clear();
        }
    }
}


