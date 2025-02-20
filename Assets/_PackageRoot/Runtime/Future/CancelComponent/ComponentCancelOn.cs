using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public abstract class ComponentCancelOn : MonoBehaviour
    {
        protected WeakAction onTrigger = new WeakAction();
        protected bool isTriggered;

        protected void Trigger()
        {
            isTriggered = true;
            onTrigger?.Invoke();
            onTrigger = null;
        }
        internal void Register<T>(IFuture<T> future)
        {
            if (isTriggered)
            {
                future.Cancel();
                return;
            }
            onTrigger += future.Cancel;
        }

        internal void Register<T>(Reference<T> future)
        {
            if (isTriggered)
            {
                future.Dispose();
                return;
            }
            onTrigger += future.Dispose;
        }
    }
}
