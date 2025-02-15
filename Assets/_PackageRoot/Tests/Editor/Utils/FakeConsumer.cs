

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public class FakeConsumer<T>
    {
        T value;

        public void Set(T value)
        {
            this.value = value;
        }

        public static void Setter(FakeConsumer<T> consumer, T value) => consumer.Set(value);
    }
}