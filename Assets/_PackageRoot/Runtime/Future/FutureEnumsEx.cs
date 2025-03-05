namespace Extensions.Unity.ImageLoader
{
    public static class FutureEnumEx
    {
        public static FutureStatus AsFutureStatus(this FutureLoadedFrom value) => (FutureStatus)value;
        public static FutureStatus AsFutureStatus(this FutureLoadingFrom value) => (FutureStatus)value;
        public static FutureStatus AsFutureStatus(this FuturePlaceholderTrigger value) => (FutureStatus)value;

        public static FutureLoadedFrom AsFutureLoadedFrom(this FutureStatus value) => (FutureLoadedFrom)value;
        public static FutureLoadedFrom AsFutureLoadedFrom(this FutureLoadingFrom value) => (FutureLoadedFrom)value;
        public static FutureLoadedFrom AsFutureLoadedFrom(this FuturePlaceholderTrigger value) => (FutureLoadedFrom)value;

        public static FutureLoadingFrom AsFutureLoadingFrom(this FutureStatus value) => (FutureLoadingFrom)value;
        public static FutureLoadingFrom AsFutureLoadingFrom(this FutureLoadedFrom value) => (FutureLoadingFrom)value;
        public static FutureLoadingFrom AsFutureLoadingFrom(this FuturePlaceholderTrigger value) => (FutureLoadingFrom)value;

        public static FuturePlaceholderTrigger AsFuturePlaceholderTrigger(this FutureStatus value) => (FuturePlaceholderTrigger)value;
        public static FuturePlaceholderTrigger AsFuturePlaceholderTrigger(this FutureLoadedFrom value) => (FuturePlaceholderTrigger)value;
        public static FuturePlaceholderTrigger AsFuturePlaceholderTrigger(this FutureLoadingFrom value) => (FuturePlaceholderTrigger)value;


        public static bool IsEqual(this FutureStatus value1, FutureLoadedFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureStatus value1, FutureLoadingFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureStatus value1, FuturePlaceholderTrigger value2) => (int)value1 == (int)value2;

        public static bool IsEqual(this FutureLoadedFrom value1, FutureStatus value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadedFrom value1, FutureLoadingFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadedFrom value1, FuturePlaceholderTrigger value2) => (int)value1 == (int)value2;

        public static bool IsEqual(this FutureLoadingFrom value1, FutureStatus value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadingFrom value1, FutureLoadedFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FutureLoadingFrom value1, FuturePlaceholderTrigger value2) => (int)value1 == (int)value2;

        public static bool IsEqual(this FuturePlaceholderTrigger value1, FutureStatus value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FuturePlaceholderTrigger value1, FutureLoadedFrom value2) => (int)value1 == (int)value2;
        public static bool IsEqual(this FuturePlaceholderTrigger value1, FutureLoadingFrom value2) => (int)value1 == (int)value2;
    }
}
