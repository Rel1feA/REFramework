using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace RECode
{
    namespace REFramework
    {
        public class TimerManager:MonoSingleton<TimerManager>
        {
            private Dictionary<string,Timer> timerDic;
            private List<string> toRemoveTimers;

            protected override void Awake()
            {
                base.Awake();
                timerDic = new Dictionary<string,Timer>();
                toRemoveTimers = new List<string>();
            }

            private void Update()
            {
                foreach(Timer timer in timerDic.Values)
                {
                    timer.CumulativeTime(Time.deltaTime);
                    timer.CheckDoAction();
                }
            }

            private void LateUpdate()
            {
                toRemoveTimers.Clear();
                foreach (var kvp in timerDic)
                {
                    if (kvp.Value.hasDoAction)
                    {
                        toRemoveTimers.Add(kvp.Key);
                    }
                }
                foreach (string key in toRemoveTimers)
                {
                    timerDic.Remove(key);
                }
            }

            public void AddTimer(string timerName,float timeToDo,UnityAction action)
            {
                if(timerDic.ContainsKey(timerName))
                {
                    Debug.Log($"已存在名为{timerName}的计时器，无法重复创建");
                    return;
                }
                Timer timer=new Timer(timeToDo,action);
                timerDic.Add(timerName, timer);
            }

            public void AddTimer(string timerName, float timeToDo, UnityAction action,bool isLoop,int loopNum)
            {
                if (timerDic.ContainsKey(timerName))
                {
                    Debug.Log($"已存在名为{timerName}的计时器，无法重复创建");
                    return;
                }
                Timer timer = new Timer(timeToDo, action,isLoop,loopNum);
                timerDic.Add(timerName, timer);
            }

            public Timer GetTimer(string timerName)
            {
                if (timerDic.TryGetValue(timerName, out Timer timer))
                {
                    return timer;
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");
                    return null;
                }
            }

            public void AddActionToTimer(string timerName,UnityAction action)
            {
                if (timerDic.TryGetValue(timerName, out Timer timer))
                {
                    timer.AddAction(action);
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");

                }
            }

            public void RemoveActionFromTimer(string timerName, UnityAction action)
            {
                if (timerDic.TryGetValue(timerName, out Timer timer))
                {
                    timer.RemoveAction(action);
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");

                }
            }

            public void PauseTimer(string timerName)
            {
                if(timerDic.TryGetValue(timerName, out Timer timer))
                {
                    timer.SetIsPause(true);
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");
                }
            }

            public void ResumeTimer(string timerName)
            {
                if (timerDic.TryGetValue(timerName, out Timer timer))
                {
                    timer.SetIsPause(false);
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");
                }
            }

            public void ResetTimer(string timerName)
            {
                if (timerDic.TryGetValue(timerName, out Timer timer))
                {
                    timer.ClearTimer();
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");
                }
            }

            public void RemoveTimer(string timerName)
            {
                if (timerDic.TryGetValue(timerName, out Timer timer))
                {
                    timerDic.Remove(timerName);
                }
                else
                {
                    Debug.Log($"未找到名为{timerName}的计时器");
                }
            }

            public void ResumeAllTimer()
            {
                if (timerDic != null && timerDic.Count > 0)
                {
                    foreach (Timer timer in timerDic.Values)
                    {
                        timer.SetIsPause(false);
                    }
                }
            }

            public void PauseAllTimer()
            {
                if(timerDic!=null&&timerDic.Count>0)
                {
                    foreach(Timer timer in timerDic.Values)
                    {
                        timer.SetIsPause(true);
                    }
                }
            }

            public void RemoveAllTimer()
            {
                timerDic.Clear();
            }
        }

        public class Timer
        {
            private float timer;
            private float timeToDo;
            private UnityAction action;
            private bool isPause;
            private bool isLoop;
            private int loopNum;
            private int loopCount;

            public bool hasDoAction;
            public float remainTime
            {
                get
                {
                    return Mathf.Max(0f, timeToDo - timer);
                }
            }

            public float progress
            {
                get
                {
                    return Mathf.Clamp01(timer/timeToDo);
                }
            }
            
            public Timer(float timeToDo,UnityAction action)
            {
                this.timeToDo = timeToDo;
                this.action = action;
                timer = 0;
                isPause= false;
                hasDoAction= false;
                isLoop= false;
            }

            public Timer(float timeToDo,UnityAction action,bool isLoop,int loopNum)
            {
                this.timeToDo = timeToDo;
                this.action = action;
                timer = 0;
                isPause = false;
                hasDoAction = false;
                this.isLoop = isLoop;
                this.loopNum = loopNum;
            }

            public void ClearTimer()
            {
                timer = 0;
                hasDoAction = false;
                loopCount = 0;
            }

            public void SetIsPause(bool value)
            {
                isPause = value;
            }

            public void CumulativeTime(float delta)
            {
                if (!isPause)
                {
                    timer += delta;
                }
            }

            public void CheckDoAction()
            {
                if(timer>=timeToDo&&!hasDoAction)
                {
                    action?.Invoke();
                    if(isLoop&&++loopCount<loopNum)
                    {
                        timer -=timeToDo;
                        return;
                    }
                    hasDoAction= true;
                }    
            }

            public void AddAction(UnityAction action)
            {
                this.action += action;
            }

            public void RemoveAction(UnityAction action)
            {
                this.action -= action;
            }
        }
    }
}


