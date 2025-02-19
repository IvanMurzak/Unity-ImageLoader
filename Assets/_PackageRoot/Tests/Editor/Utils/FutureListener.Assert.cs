using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public partial class FutureListener<T>
    {
        public FutureListener<T> Assert_Events_Equals(params EventName[] eventNames) => Assert_Events_Equals(eventNames.ToList());
        public FutureListener<T> Assert_Events_Equals(IReadOnlyList<EventName> eventNames)
        {
            var events = Events;
            if (events.Count != eventNames.Count)
                throw new Exception($"Expected {eventNames.Count} events, but got {events.Count}.\nExpected: {string.Join(", ", eventNames)}\nGot: {string.Join(", ", events.Select(x => x.name))}");

            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].name != eventNames[i])
                {
                    throw new Exception($"[{i}] Expected event {eventNames[i]}, but got {events[i].name}");
                }
            }
            return this;
        }
        public FutureListener<T> Assert_Events_Count(int count, EventName eventName)
        {
            var events = Events;
            var realCount = events.Count(x => x.name == eventName);
            if (realCount != count)
            {
                throw new Exception($"Expected {count} events of type {eventName}, but got {realCount}");
            }
            return this;
        }
        public FutureListener<T> Assert_Events_NoRepeats(int count, EventName eventName)
        {
            var events = Events;
            foreach (var group in events.GroupBy(x => x.name))
            {
                if (group.Count() > count)
                {
                    throw new Exception($"Expected at most {count} events of type {eventName}, but got {group.Count()}");
                }
            }
            return this;
        }
        public FutureListener<T> Assert_Events_Contains(EventName eventName)
        {
            var events = Events;
            if (!events.Any(x => x.name == eventName))
            {
                throw new Exception($"Expected event {eventName}, but it was not found");
            }
            return this;
        }
        public FutureListener<T> Assert_Events_NotContains(EventName eventName)
        {
            if (events.Any(x => x.name == eventName))
            {
                throw new Exception($"Expected event {eventName}, but it was not found");
            }
            return this;
        }
        public FutureListener<T> Assert_Events_Contains(EventName eventName, object value)
        {
            var events = Events;
            if (!events.Any(x => x.name == eventName && x.value == value))
            {
                throw new Exception($"Expected event {eventName} with value {value}, but it was not found");
            }
            return this;
        }
        public FutureListener<T> Assert_Events_Value(EventName eventName, Func<object, bool> validate)
        {
            var events = Events;
            if (!events.Any(x => x.name == eventName && validate(x.value)))
            {
                throw new Exception($"Expected event {eventName} with value that passes the validation, but it was not found");
            }
            return this;
        }
        public FutureListener<T> Assert_Events_Value<V>(EventName eventName, Func<V, bool> validate)
        {
            var events = Events;
            if (!events.Any(x => x.name == eventName && validate(((V)x.value))))
            {
                throw new Exception($"Expected event {eventName} with value that passes the validation, but it was not found");
            }
            return this;
        }
    }
}