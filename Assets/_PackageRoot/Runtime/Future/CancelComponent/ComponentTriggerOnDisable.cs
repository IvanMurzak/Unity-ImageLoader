namespace Extensions.Unity.ImageLoader
{
    internal class ComponentTriggerOnDisable : ComponentTrigger
    {
        private void OnDisable() => Trigger();
    }
}
