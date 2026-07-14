using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace RECode.REFramework
{
    public class ConfigManager : NormalSingleton<ConfigManager>
    {
        private Dictionary<Type,object> tableCache=new Dictionary<Type,object>();

        public List<T> LoadCsv<T>(string fileName)where T : class, new()
        {
            Type type = typeof(T);
            if(tableCache.ContainsKey(type))
                return tableCache[type] as List<T>;
            var rawData=CsvReader.Instance.LoadFromStreamingAssets(fileName);
            if (rawData == null || rawData.Count < 2) return null;
            string[] headers = rawData[0];
            var list = new List<T>();
            for(int row=1; row<rawData.Count; row++)
            {
                string[] rowData = rawData[row];
                T obj=new T();
                for(int i=0;i<headers.Length;i++)
                {
                    string fieldName=headers[i];
                    string value=rowData[i];
                    SetPropertyOrField(obj, fieldName, value);
                }
                list.Add(obj);
            }
            tableCache[type]=list;
            return list;
        }

        /// <summary>
        /// 读取Config数据
        /// </summary>
        /// <typeparam name="T">目标数据类</typeparam>
        /// <param name="fileName">表格文件名</param>
        /// <param name="headerIndex">变量名所在的行</param>
        /// <param name="ignoreCol">忽略某列的数据</param>
        /// <returns></returns>
        public List<T> LoadCsv<T>(string fileName,int headerIndex,int ignoreCol=-1) where T : class, new()
        {
            Type type = typeof(T);
            if (tableCache.ContainsKey(type))
                return tableCache[type] as List<T>;
            var rawData = CsvReader.Instance.LoadFromStreamingAssets(fileName);
            if (rawData == null || rawData.Count < 2) return null;
            string[] headers = rawData[headerIndex];
            var list = new List<T>();
            for (int row = 1; row < rawData.Count; row++)
            {
                string[] rowData = rawData[row];
                T obj = new T();
                for (int i = 0; i < headers.Length; i++)
                {
                    if(i==ignoreCol) continue;
                    string fieldName = headers[i];
                    string value = rowData[i];
                    SetPropertyOrField(obj, fieldName, value);
                }
                list.Add(obj);
            }
            tableCache[type] = list;
            return list;
        }

        private void SetPropertyOrField(object obj,string name,string value)
        {
            var type=obj.GetType();
            var prop=type.GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
            if(prop != null && prop.CanWrite)
            {
                object converted=ConvertEmptyString(value,prop.PropertyType);
                prop.SetValue(obj,converted);
                return;
            }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if(field!=null)
            {
                object converted= ConvertEmptyString(value,field.FieldType);
                field.SetValue(obj,converted);
                return;
            }
        }

        private object ConvertEmptyString(string value,Type targetType)
        {
            if(string.IsNullOrEmpty(value))
            {
                 Debug.LogWarning($"表格出现空格，{targetType}赋值默认值！");
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType)==null)
                    return Activator.CreateInstance(targetType);
                else
                    return null;
            }
            return Convert.ChangeType(value, targetType);
        }
    }

}


