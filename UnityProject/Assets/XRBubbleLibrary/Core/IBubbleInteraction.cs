using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for bubble interaction components
    /// Allows Physics assembly to communicate with Interactions assembly without circular dependency
    /// </summary>
    public interface IBubbleInteraction
    {
        /// <summary>
        /// Get the transform of the bubble for physics calculations
        /// </summary>
        Transform BubbleTransform { get; }
        
        /// <summary>
        /// Get the renderer component for visual effects
        /// </summary>
        Renderer BubbleRenderer { get; }
        
        /// <summary>
        /// Check if the bubble is currently being interacted with
        /// </summary>
        bool IsBeingInteracted { get; }
        
        /// <summary>
        /// Get the current interaction position (if any)
        /// </summary>
        Vector3 InteractionPosition { get; }
        
        /// <summary>
        /// Apply physics force to the bubble
        /// </summary>
        /// <param name="force">Force to apply</param>
        void ApplyPhysicsForce(Vector3 force);
        
        /// <summary>
        /// Update the bubble's visual state based on physics
        /// </summary>
        /// <param name="scale">New scale from breathing animation</param>
        void UpdateVisualScale(float scale);
    }
}