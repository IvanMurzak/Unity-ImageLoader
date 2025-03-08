namespace Extensions.Unity.ImageLoader
{
    public static class FutureEnumEx
    {
        public static FutureStatus AsFutureStatus(this FutureLoadedFrom value) => (FutureStatus)value;
        public static FutureStatus AsFutureStatus(this FutureLoadingFrom value) => (FutureStatus)value;
        public static FutureStatus AsFutureStatus(this PlaceholderTrigger value) => (FutureStatus)value;

        public static FutureLoadedFrom AsFutureLoadedFrom(this FutureStatus value) => (FutureLoadedFrom)value;
        public static FutureLoadedFrom AsFutureLoadedFrom(this FutureLoadingFrom value) => (FutureLoadedFrom)value;
        public static FutureLoadedFrom AsFutureLoadedFrom(this PlaceholderTrigger value) => (FutureLoadedFrom)value;

        public static FutureLoadingFrom AsFutureLoadingFrom(this FutureStatus value) => (FutureLoadingFrom)value;
        public static FutureLoadingFrom AsFutureLoadingFrom(this FutureLoadedFrom value) => (FutureLoadingFrom)value;
        public static FutureLoadingFrom AsFutureLoadingFrom(this PlaceholderTrigger value) => (FutureLoadingFrom)value;

        public static PlaceholderTrigger AsPlaceholderTrigger(this FutureStatus value) => (PlaceholderTrigger)value;
        public static PlaceholderTrigger AsPlaceholderTrigger(this FutureLoadedFrom value) => (PlaceholderTrigger)value;
        public static PlaceholderTrigger AsPlaceholderTrigger(this FutureLoadingFrom value) => (PlaceholderTrigger)value;


        public static bool IsEqual(this FutureStatus value1, FutureLoadedFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureStatus value1, FutureLoadingFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureStatus value1, PlaceholderTrigger value2) => (int)value1 == (int)value2;

        public static bool IsEqual(this FutureLoadedFrom value1, FutureStatus value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadedFrom value1, FutureLoadingFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadedFrom value1, PlaceholderTrigger value2) => (int)value1 == (int)value2;

        public static bool IsEqual(this FutureLoadingFrom value1, FutureStatus value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadingFrom value1, FutureLoadedFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadingFrom value1, PlaceholderTrigger value2) => (int)value1 == (int)value2;

        public static bool IsEqual(this PlaceholderTrigger value1, FutureStatus value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this PlaceholderTrigger value1, FutureLoadedFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this PlaceholderTrigger value1, FutureLoadingFrom value2) => (int)value1 == (int)value2;
    }
}
