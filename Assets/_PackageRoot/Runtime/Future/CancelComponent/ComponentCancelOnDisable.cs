namespace Extensions.Unity.ImageLoader
{
    internal class ComponentCancelOnDisable : ComponentCancelOn
    {
        private void OnDisable() => Trigger();
    }
}
