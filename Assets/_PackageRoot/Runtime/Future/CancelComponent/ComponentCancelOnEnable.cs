namespace Extensions.Unity.ImageLoader
{
    internal class ComponentCancelOnEnable : ComponentCancelOn
    {
        private void OnEnable() => Trigger();
    }
}
