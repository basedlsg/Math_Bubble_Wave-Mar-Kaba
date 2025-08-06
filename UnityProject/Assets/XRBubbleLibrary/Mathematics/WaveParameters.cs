using Unity.Mathematics;

namespace XRBubbleLibrary.Mathematics
{
    [System.Serializable]
    public struct WaveParameters
    {
        public float primaryFrequency;
        public float primaryAmplitude;
        public float primaryPhase;
        public float secondaryFrequency;
        public float secondaryAmplitude;
        public float secondaryPhase;
        public float tertiaryFrequency;
        public float tertiaryAmplitude;
        public float tertiaryPhase;
        public float baseHeight;

        public static WaveParameters Default => new WaveParameters
        {
            primaryFrequency = 1.0f,
            primaryAmplitude = 0.2f,
            secondaryFrequency = 2.5f,
            secondaryAmplitude = 0.1f,
            tertiaryFrequency = 5.0f,
            tertiaryAmplitude = 0.05f,
            baseHeight = 1.0f
        };
    }
}
