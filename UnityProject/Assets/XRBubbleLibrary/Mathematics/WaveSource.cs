using Unity.Mathematics;

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Defines a wave source for interference calculations
    /// Used by WavePatternGenerator for multi-source wave simulation
    /// </summary>
    [System.Serializable]
    public struct WaveSource
    {
        /// <summary>
        /// 3D position of the wave source
        /// </summary>
        public float3 position;
        
        /// <summary>
        /// Wave frequency in Hz
        /// </summary>
        public float frequency;
        
        /// <summary>
        /// Wave amplitude (strength)
        /// </summary>
        public float amplitude;
        
        /// <summary>
        /// Phase offset for the wave
        /// </summary>
        public float phase;
        
        /// <summary>
        /// Create a new wave source
        /// </summary>
        /// <param name="pos">Position of the source</param>
        /// <param name="freq">Frequency in Hz</param>
        /// <param name="amp">Amplitude of the wave</param>
        /// <param name="phaseOffset">Phase offset</param>
        public WaveSource(float3 pos, float freq, float amp, float phaseOffset = 0f)
        {
            position = pos;
            frequency = freq;
            amplitude = amp;
            phase = phaseOffset;
        }
        
        /// <summary>
        /// Default wave source with reasonable values
        /// </summary>
        public static WaveSource Default => new WaveSource(
            new float3(0, 0, 0), 
            1.0f, 
            0.1f, 
            0f
        );
    }
}