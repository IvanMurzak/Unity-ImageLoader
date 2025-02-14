using Extensions.Unity.ImageLoader.Utils;
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
