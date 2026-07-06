using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RECode.REFramework
{
    public class ResourcesManager : NormalSingleton<ResourcesManager>
    {
        private bool isUnloading=false;

        //同步加载资源
        public T Load<T>(string path) where T : Object
        {
            T res = Resources.Load<T>(path);
            if (res is GameObject)
            {
                return GameObject.Instantiate(res);
            }
            else
            {
                return res;
            }
        }


        //异步加载资源
        public void LoadAsync<T>(string path, UnityAction<T> callback) where T : Object
        {
            MonoController.Instance.StartCoroutine(ReallyLoadAsync(path, callback));
        }

        private IEnumerator ReallyLoadAsync<T>(string path, UnityAction<T> callback) where T : Object
        {
            ResourceRequest r = Resources.LoadAsync<T>(path);
            yield return r;
            if (r.asset is GameObject)
            {
                callback(GameObject.Instantiate(r.asset) as T);
            }
            else
            {
                callback(r.asset as T);
            }
        }

        public void UnloadAsset(Object asset)
        {
            if (asset != null)
            {
                Resources.UnloadAsset(asset);
            }
        }

        public void UnloadUnusedAssets(UnityAction callback=null)
        {
            if(isUnloading)return;
            MonoController.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
        }

        private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback=null)
        {
            isUnloading = true;
            AsyncOperation op= Resources.UnloadUnusedAssets();
            yield return op;
            isUnloading= false;
            callback?.Invoke();
        }
    }
    
}


