using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> dataDic = new Dictionary<string, object>();

        public void SetValue<T>(string key, T value)
        {
            dataDic[key] = value;
        }

        public bool TrySetValue<T>(string key, T value)
        {
            if (dataDic.TryGetValue(key, out object data))
            {
                if (data is T)
                {
                    dataDic[key] = value;
                    return true;
                }
                Debug.LogError($"[Blackboard] Key '{key}' 类型不匹配");
                return false;
            }
            dataDic[key] = value;
            return true;
        }

        public T GetValue<T>(string key)
        {
            if (dataDic.TryGetValue(key, out var value) && value is T)
                return (T)value;
            return default;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (dataDic.TryGetValue(key, out var data) && data is T)
            {
                value = (T)data;
                return true;
            }
            value = default;
            return false;
        }

        public bool HasKey(string key)
        {
            return dataDic.ContainsKey(key);
        }

        public void RemoveKey(string key)
        {
            dataDic.Remove(key);
        }

        public void Clear()
        {
            dataDic.Clear();
        }
    }
}



