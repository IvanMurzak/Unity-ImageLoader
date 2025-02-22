using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions.Unity.ImageLoader
{
    public sealed class WeakAction<T>
    {
        List<WeakReference<Action<T>>> listeners = new List<WeakReference<Action<T>>>();

        public void AddListener(Action<T> handler)
        {
            if (handler == null)
                return;

            lock (listeners)
                listeners.Add(new WeakReference<Action<T>>(handler, trackResurrection: false));
        }

        public void RemoveListener(Action<T> handler)
        {
            if (handler == null)
                return;

            lock (listeners)
                listeners.RemoveAll(wr => !wr.TryGetTarget(out var action) || handler.Equals(action));
        }

        public void Invoke(T data)
        {
            lock (listeners)
            {
                var subscribers = listeners
                    .Select(x =>
                    {
                        var alive = x.TryGetTarget(out var action);
                        return (alive, action);
                    })
                    .ToArray();

                var isAnyExpired = false;
                foreach (var subscriber in subscribers)
                {
                    if (subscriber.alive)
                        Safe.Run(subscriber.action, data, DebugLevel.Exception);
                    else
                        isAnyExpired = true;
                }
                if (isAnyExpired)
                    listeners.RemoveAll(wr => !wr.TryGetTarget(out var action));
            }
        }

        public static WeakAction<T> operator +(WeakAction<T> weakEvent, Action<T> handler)
        {
            weakEvent.AddListener(handler);
            return weakEvent;
        }

        public static WeakAction<T> operator -(WeakAction<T> weakEvent, Action<T> handler)
        {
            weakEvent.RemoveListener(handler);
            return weakEvent;
        }
    }

    public sealed class WeakAction
    {
        List<WeakReference<Action>> listeners = new List<WeakReference<Action>>();

        public void AddListener(Action handler)
        {
            if (handler == null)
                return;

            lock (listeners)
                listeners.Add(new WeakReference<Action>(handler, trackResurrection: false));
        }

        public void RemoveListener(Action handler)
        {
            if (handler == null)
                return;

            lock (listeners)
                listeners.RemoveAll(wr => !wr.TryGetTarget(out var action) || handler.Equals(action));
        }

        public void Invoke()
        {
            lock (listeners)
            {
                var subscribers = listeners
                    .Select(x =>
                    {
                        var alive = x.TryGetTarget(out var action);
                        return (alive, action);
                    })
                    .ToArray();

                var isAnyExpired = false;
                foreach (var subscriber in subscribers)
                {
                    if (subscriber.alive)
                        Safe.Run(subscriber.action, DebugLevel.Exception);
                    else
                        isAnyExpired = true;
                }
                if (isAnyExpired)
                    listeners.RemoveAll(wr => !wr.TryGetTarget(out var action));
            }
        }

        public static WeakAction operator +(WeakAction weakEvent, Action action)
        {
            weakEvent.AddListener(action);
            return weakEvent;
        }

        public static WeakAction operator -(WeakAction weakEvent, Action action)
        {
            weakEvent.RemoveListener(action);
            return weakEvent;
        }
    }
}