using UnityEngine;
using UnityEngine.XR;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// Data structure for XR interaction events.
    /// </summary>
    [System.Serializable]
    public struct XRInteractionData
    {
        public Vector3 ContactPoint;
        public float PressureIntensity;
        public InputDevice InputSource;
        public float InteractionDuration;
    }
}
