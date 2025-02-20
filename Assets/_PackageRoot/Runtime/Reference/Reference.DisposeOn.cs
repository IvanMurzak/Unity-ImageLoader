using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ReferenceEx
    {
        /// <summary>
        /// Subscribe on any event of GameObject, cancel the Reference at this event.
        /// You may need to create a class extended from ComponentCancelOn to subscribe on a custom event.
        /// Or use other DisposeOn function version with already implemented list of events.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOn<T, K>(this Reference<T> future, Component component) where K : ComponentCancelOn
        {
            return DisposeOn<T, K>(future, component.gameObject);
        }

        /// <summary>
        /// Subscribe on any event of GameObject, cancel the Reference at this event.
        /// You may need to create a class extended from ComponentCancelOn to subscribe on a custom event.
        /// Or use other DisposeOn function version with already implemented list of events.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOn<T, K>(this Reference<T> future, GameObject gameObject) where K : ComponentCancelOn
        {
            var cancellation = gameObject.GetComponent<K>() ?? gameObject.AddComponent<K>();
            cancellation.Register(future);
            return future;
        }


        /// <summary>
        /// Subscribe on OnDestroy event of GameObject, cancel the Reference at this event.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOnDestroy<T>(this Reference<T> reference, Component component) => DisposeOn<T, ComponentCancelOnDestroy>(reference, component);

        /// <summary>
        /// Subscribe on OnDestroy event of GameObject, cancel the Reference at this event.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOnDestroy<T>(this Reference<T> reference, GameObject gameObject) => DisposeOn<T, ComponentCancelOnDestroy>(reference, gameObject);


        /// <summary>
        /// Subscribe on OnDisable event of GameObject, cancel the Reference at this event.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOnDisable<T>(this Reference<T> reference, Component component) => DisposeOn<T, ComponentCancelOnDisable>(reference, component);

        /// <summary>
        /// Subscribe on OnDisable event of GameObject, cancel the Reference at this event.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOnDisable<T>(this Reference<T> reference, GameObject gameObject) => DisposeOn<T, ComponentCancelOnDisable>(reference, gameObject);


        /// <summary>
        /// Subscribe on OnEnable event of GameObject, cancel the Reference at this event.
        /// </summary>
        /// <param name="component">target component to subscribe on it's gameObject</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOnEnable<T>(this Reference<T> reference, Component component) => DisposeOn<T, ComponentCancelOnEnable>(reference, component);

        /// <summary>
        /// Subscribe on OnEnable event of GameObject, cancel the Reference at this event.
        /// </summary>
        /// <param name="gameObject">target component to subscribe on it</param>
        /// <returns>Returns async Reference</returns>
        public static Reference<T> DisposeOnEnable<T>(this Reference<T> reference, GameObject gameObject) => DisposeOn<T, ComponentCancelOnEnable>(reference, gameObject);
    }
}
