namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Static registry for providing IWaveOptimizer instances.
    /// This allows the Mathematics assembly to access AI optimization services
    /// without a direct dependency on the AI assembly, thus preventing
    /// circular references.
    /// </summary>
    public static class WaveOptimizationRegistry
    {
        private static IWaveOptimizer optimizer;

        public static bool IsOptimizationAvailable => optimizer != null;

        public static void RegisterOptimizer(IWaveOptimizer instance)
        {
            optimizer = instance;
        }

        public static void UnregisterOptimizer()
        {
            optimizer = null;
        }

        public static IWaveOptimizer GetOptimizer()
        {
            return optimizer;
        }
    }
}