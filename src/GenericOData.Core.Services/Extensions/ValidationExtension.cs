namespace GenericOData.Core.Services.Extensions
{
    public static class ValidationExtension
    {
        public static void Required<T>(this T instance, string argumentName) where T : class
        {
            if (instance is null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void Required<T>(this T? instance, string argumentName) where T : struct
        {
            if (!instance.HasValue)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void Required(this string instance, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
