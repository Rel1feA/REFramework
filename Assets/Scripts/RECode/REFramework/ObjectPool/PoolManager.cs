using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RECode
{
    namespace REFramework
    {
        public class PoolData
        {
            public GameObject fatherObj;
            public Stack<GameObject> poolStack;

            public PoolData(GameObject obj, GameObject poolObj)
            {
                fatherObj = new GameObject(obj.name+"Root");
                fatherObj.transform.parent = poolObj.transform;
                poolStack = new Stack<GameObject>();
                PushObj(obj);
            }

            public GameObject GetObj()
            {
                GameObject obj = null;
                obj = poolStack.Pop();
                obj.SetActive(true);
                obj.transform.parent = null;
                return obj;
            }

            public void PushObj(GameObject obj)
            {
                obj.SetActive(false);
                poolStack.Push(obj);
                obj.transform.parent = fatherObj.transform;
            }
        }


        public class PoolManager : MonoSingleton<PoolManager>
        {
            public Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
            private GameObject poolObj;
            private const string path = "Prefabs/";

            public void GetObj(string name, UnityAction<GameObject> callback)
            {
                if (poolDic.ContainsKey(name) && poolDic[name].poolStack.Count > 0)
                {
                    callback(poolDic[name].GetObj());
                }
                else
                {
                    ResourcesManager.Instance.LoadAsync<GameObject>(path+name, ((o) =>
                    {
                        o.name = name;
                        callback(o);
                    }));
                }
            }

            public void PushObj(string name, GameObject obj)
            {
                if (poolObj == null)
                {
                    poolObj = new GameObject("Pool");
                }
                if (poolDic.ContainsKey(name))
                {
                    poolDic[name].PushObj(obj);
                }
                else
                {
                    poolDic.Add(name, new PoolData(obj, poolObj));
                }
            }

            public void WarmPool(string name,int num=25)
            {
                for(int i=0;i<num; i++)
                {
                    ResourcesManager.Instance.LoadAsync<GameObject>(path + name, (o) =>
                    {
                        o.name = name;
                        PushObj(name, o);
                    });
                }
            }

            public void Clear()
            {
                poolDic.Clear();
                poolObj = null;
            }
        }
    }
}

