using System;
using System.Runtime.CompilerServices;

namespace Extensions.Unity.ImageLoader
{
    public class FutureAwaiter<T> : INotifyCompletion
    {
        private readonly TaskAwaiter<T> _awaiter;
        public FutureAwaiter(TaskAwaiter<T> awaiter) { _awaiter = awaiter; }
        public bool IsCompleted => _awaiter.IsCompleted;
        public T GetResult() => _awaiter.GetResult();
        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
    }
}