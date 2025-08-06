namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Static registry for providing IBiasFieldProvider instances.
    /// This allows the Mathematics assembly to access AI bias field generation
    /// services without a direct dependency on the AI assembly.
    /// </summary>
    public static class BiasFieldRegistry
    {
        private static IBiasFieldProvider provider;

        public static bool IsAvailable => provider != null;

        public static void RegisterProvider(IBiasFieldProvider instance)
        {
            provider = instance;
        }

        public static IBiasFieldProvider GetProvider()
        {
            return provider;
        }
    }
}