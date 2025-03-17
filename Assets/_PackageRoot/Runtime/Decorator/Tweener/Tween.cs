using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;

namespace Extensions.Unity.ImageLoader
{
    public abstract class Tween<C, T> : IDisposable
    {
        public enum TweenType
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            Spring
        }

        protected float duration;
        protected float elapsedTime;
        protected bool isPlaying;

        protected C context;
        protected Func<C, T> getter;
        protected Action<C, T> setter;
        protected T startValue;
        protected T targetValue;
        protected TweenType tweenType;

        public Tween(C context, Func<C, T> getter, Action<C, T> setter, T targetValue, float duration, TweenType tweenType = TweenType.Linear)
        {
            this.context = context;
            this.getter = getter;
            this.setter = setter;
            this.startValue = getter(context);
            this.targetValue = targetValue;
            this.duration = duration;
            this.tweenType = tweenType;
            this.elapsedTime = 0f;
            this.isPlaying = false;
        }

        public abstract void Update(float deltaTime);

        public void Play()
        {
            startValue = getter(context); // Get the current value when starting the tween
            isPlaying = true;
            elapsedTime = 0f;
        }

        public void Stop()
        {
            isPlaying = false;
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        protected float Evaluate(float t, TweenType tweenType)
        {
            switch (tweenType)
            {
                case TweenType.Linear:
                    return t;
                case TweenType.EaseIn:
                    return t * t;
                case TweenType.EaseOut:
                    return t * (2 - t);
                case TweenType.EaseInOut:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case TweenType.Spring:
                    return 1f - Mathf.Cos(t * Mathf.PI * 4) * Mathf.Exp(-t * 6);
                default:
                    return t;
            }
        }

        public async Task PlayAsync()
        {
            Play();
            while (IsPlaying())
            {
                await Task.Yield();
            }
        }

        public virtual void Dispose()
        {
            Stop();
            context = default;
            getter = null;
            setter = null;
        }
    }
}