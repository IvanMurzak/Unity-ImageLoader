using System;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public abstract class ComponentTrigger : MonoBehaviour
    {
        // protected WeakAction onTrigger = new WeakAction();
        protected event Action onTrigger;
        protected bool isTriggered;

        protected void Trigger()
        {
            isTriggered = true;
            onTrigger?.Invoke();
            onTrigger = null;
        }
        public void RegisterCancel<T>(IFuture<T> future)
        {
            if (future.Status == FutureStatus.Disposed || future.IsCompleted)
                return; // ignore completed futures

            if (isTriggered)
            {
                future.Cancel();
                return;
            }

            // TODO: check if future is completed during the onTrigger event
            // onTrigger += future.Cancel;
            // -------------------------------------------------------------
            var weakFuture = new WeakReference<IFuture<T>>(future);
            onTrigger += () =>
            {
                if (weakFuture.TryGetTarget(out var target))
                {
                    if (future.Status == FutureStatus.Disposed || target.IsCompleted)
                        return; // ignore completed futures

                    target.Cancel();
                }
            };
        }

        public void RegisterDispose<T>(Reference<T> reference)
        {
            if (reference.IsDisposed)
                return; // ignore disposed references

            if (isTriggered)
            {
                reference.Dispose();
                return;
            }

            // TODO: check if reference is disposed during the onTrigger event
            // onTrigger += reference.Dispose;
            // -------------------------------------------------------------
            var weakReference = new WeakReference<Reference<T>>(reference);
            onTrigger += () =>
            {
                if (weakReference.TryGetTarget(out var target))
                {
                    if (target.IsDisposed)
                        return; // ignore disposed references

                    target.Dispose();
                }
            };
        }
    }
}
