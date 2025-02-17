

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public interface IConsumer<T>
    {
        T Value { get; }
        void Consume(T value);
    }
    public class FakeConsumer<T> : IConsumer<T>
    {
        public T Value { get; private set; }

        public void Consume(T value) => Value = value;

        public static void Setter(FakeConsumer<T> consumer, T value) => consumer.Consume(value);
    }
}