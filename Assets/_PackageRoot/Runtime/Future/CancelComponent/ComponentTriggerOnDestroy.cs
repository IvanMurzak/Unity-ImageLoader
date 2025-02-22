namespace Extensions.Unity.ImageLoader
{
    internal class ComponentTriggerOnDestroy : ComponentTrigger
    {
        private void OnDestroy() => Trigger();
    }
}
