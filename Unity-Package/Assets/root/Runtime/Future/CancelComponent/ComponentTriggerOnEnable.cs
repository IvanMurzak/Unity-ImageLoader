namespace Extensions.Unity.ImageLoader
{
    internal class ComponentTriggerOnEnable : ComponentTrigger
    {
        private void OnEnable() => Trigger();
    }
}
