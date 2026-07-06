using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RECode.REFramework
{
    public class MonoController : MonoSingleton<MonoController>
    {
        public event UnityAction updateEvent;

        private void Update()
        {
            if (updateEvent != null)
            {
                updateEvent();
            }
        }

        public void AddUpdateListener(UnityAction fun)
        {
            updateEvent += fun;
        }

        public void RemoveUpdateListener(UnityAction fun)
        {
            updateEvent -= fun;
        }

        public Coroutine MonoStartCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }
    }
    
}


