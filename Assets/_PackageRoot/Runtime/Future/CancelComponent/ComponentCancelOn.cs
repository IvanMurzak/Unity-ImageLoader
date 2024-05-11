using System;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public abstract class ComponentCancelOn : MonoBehaviour
    {
        protected event Action onTrigger;
        protected bool isTriggered;

        protected void Trigger()
        {
            onTrigger?.Invoke();
            onTrigger = null;
        }
        internal void Register<T>(Future<T> future)
        {
            if (isTriggered)
            {
                future.Cancel();
                return;
            }
            onTrigger += future.Cancel;
        }
    }
}
