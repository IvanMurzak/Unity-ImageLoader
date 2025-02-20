using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Subscribe on any event of GameObject, cancel the Future at this event.
        /// You may need to create a class extended from ComponentCancelOn to subscribe on a custom event.
        /// Or use other CancelOn function version with already implemented list of events.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOn<T, K>(this IFuture<T> future, Component component) where K : ComponentCancelOn
        {
            return CancelOn<T, K>(future, component.gameObject);
        }

        /// <summary>
        /// Subscribe on any event of GameObject, cancel the Future at this event.
        /// You may need to create a class extended from ComponentCancelOn to subscribe on a custom event.
        /// Or use other CancelOn function version with already implemented list of events.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOn<T, K>(this IFuture<T> future, GameObject gameObject) where K : ComponentCancelOn
        {
            var cancellation = gameObject.GetComponent<K>() ?? gameObject.AddComponent<K>();
            cancellation.Register(future);
            return future;
        }


        /// <summary>
        /// Subscribe on OnDestroy event of GameObject, cancel the Future at this event.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOnDestroy<T>(this IFuture<T> future, Component component) => CancelOn<T, ComponentCancelOnDestroy>(future, component);

        /// <summary>
        /// Subscribe on OnDestroy event of GameObject, cancel the Future at this event.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOnDestroy<T>(this IFuture<T> future, GameObject gameObject) => CancelOn<T, ComponentCancelOnDestroy>(future, gameObject);


        /// <summary>
        /// Subscribe on OnDisable event of GameObject, cancel the Future at this event.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOnDisable<T>(this IFuture<T> future, Component component) => CancelOn<T, ComponentCancelOnDisable>(future, component);

        /// <summary>
        /// Subscribe on OnDisable event of GameObject, cancel the Future at this event.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOnDisable<T>(this IFuture<T> future, GameObject gameObject) => CancelOn<T, ComponentCancelOnDisable>(future, gameObject);


        /// <summary>
        /// Subscribe on OnEnable event of GameObject, cancel the Future at this event.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOnEnable<T>(this IFuture<T> future, Component component) => CancelOn<T, ComponentCancelOnEnable>(future, component);

        /// <summary>
        /// Subscribe on OnEnable event of GameObject, cancel the Future at this event.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> CancelOnEnable<T>(this IFuture<T> future, GameObject gameObject) => CancelOn<T, ComponentCancelOnEnable>(future, gameObject);
    }
}
