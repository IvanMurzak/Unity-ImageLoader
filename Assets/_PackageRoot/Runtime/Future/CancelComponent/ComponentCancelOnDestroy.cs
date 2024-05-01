namespace Extensions.Unity.ImageLoader
{
    internal class ComponentCancelOnDestroy : ComponentCancelOn
    {
        private void OnDestroy() => Trigger();
    }
}
