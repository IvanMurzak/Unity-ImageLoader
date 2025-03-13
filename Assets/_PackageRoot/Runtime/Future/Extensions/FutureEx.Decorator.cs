namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Converts the Future to a Decorator
        /// </summary>
        /// <param name="consumer">The consumer instance</param>
        /// <returns>Returns Decorator<C, T></returns>
        public static Decorator<C, T> ToDecorator<C, T>(this IFuture<T> future, C consumer)
            => new Decorator<C, T>(consumer, future);
    }
}
