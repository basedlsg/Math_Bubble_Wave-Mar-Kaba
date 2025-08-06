using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.UI
{
    /// <summary>
    /// Spatial layout system for arranging bubbles in wave patterns around the user
    /// Cloned from Unity UI Toolkit layout samples, modified for 3D spatial arrangement
    /// </summary>
    public class SpatialBubbleLayout : MonoBehaviour
    {
        [Header("Wave Pattern Configuration")]
        [Range(0.5f, 3.0f)]
        public float waveRadius = 1.5f;
        
        [Range(0.1f, 1.0f)]
        public float waveAmplitude = 0.3f;
        
        [Range(0.5f, 3.0f)]
        public float waveFrequency = 1.0f;
        
        [Range(0.0f, 360.0f)]
        public float wavePhaseOffset = 0.0f;
        
        [Header("Spatial Arrangement")]
        [Range(3, 20)]
        public int bubblesPerRing = 8;
        
        [Range(1, 5)]
        public int numberOfRings = 3;
        
        [Range(0.2f, 1.0f)]
        public float ringSpacing = 0.4f;
        
        [Header("User-Centric Positioning")]
        public Transform userHead; // XR Camera/Head transform
        
        [Range(0.5f, 2.0f)]
        public float heightOffset = 0.0f; // Relative to user eye level
        
        [Range(-30f, 30f)]
        public float verticalAngle = 0.0f; // Tilt the entire layout
        
        [Header("Dynamic Behavior")]
        public bool enableBreathing = true;
        
        [Range(0.2f, 0.5f)]
        public float breathingFrequency = 0.3f;
        
        public bool enableWaveMotion = true;
        
        [Range(0.1f, 0.5f)]
        public float waveMotionSpeed = 0.2f;
        
        // Internal state
        private List<Transform> bubbleTransforms = new List<Transform>();
        private List<Vector3> basePositions = new List<Vector3>();
        private float animationTime = 0.0f;
        
        // Performance optimization
        private bool isDirty = true;
        private float lastUpdateTime = 0.0f;
        private const float updateInterval = 0.016f; // ~60 FPS
        
        void Start()
        {
            InitializeSpatialLayout();
        }
        
        void Update()
        {
            // Performance optimization - limit update frequency
            if (Time.time - lastUpdateTime < updateInterval)
                return;
                
            lastUpdateTime = Time.time;
            animationTime += Time.deltaTime;
            
            if (enableBreathing || enableWaveMotion || isDirty)
            {
                UpdateBubblePositions();
                isDirty = false;
            }
        }
        
        /// <summary>
        /// Initialize the spatial layout system
        /// Based on cloned Unity UI Toolkit layout patterns
        /// </summary>
        void InitializeSpatialLayout()
        {
            if (userHead == null)
            {
                // Try to find XR camera if not assigned
                Camera xrCamera = FindObjectOfType<Camera>();
                if (xrCamera != null)
                    userHead = xrCamera.transform;
                else
                    userHead = transform; // Fallback to this transform
            }
            
            CalculateBasePositions();
            CollectBubbleTransforms();
        }
        
        /// <summary>
        /// Calculate base positions for bubbles in wave pattern
        /// Uses mathematical wave functions for natural spatial arrangement
        /// </summary>
        void CalculateBasePositions()
        {
            basePositions.Clear();
            
            for (int ring = 0; ring < numberOfRings; ring++)
            {
                float ringRadius = waveRadius + (ring * ringSpacing);
                int bubblesInThisRing = bubblesPerRing + (ring * 2); // More bubbles in outer rings
                
                for (int i = 0; i < bubblesInThisRing; i++)
                {
                    // Calculate angle around the ring
                    float angle = (i / (float)bubblesInThisRing) * 2.0f * Mathf.PI;
                    
                    // Add wave phase offset for variety
                    float phaseOffset = (wavePhaseOffset * Mathf.Deg2Rad) + (ring * 0.5f);
                    
                    // Calculate base position in ring
                    float x = Mathf.Sin(angle + phaseOffset) * ringRadius;
                    float z = Mathf.Cos(angle + phaseOffset) * ringRadius;
                    
                    // Add wave amplitude variation
                    float waveHeight = Mathf.Sin(angle * waveFrequency + phaseOffset) * waveAmplitude;
                    float y = heightOffset + waveHeight;
                    
                    // Apply vertical angle tilt
                    if (verticalAngle != 0.0f)
                    {
                        float tiltRad = verticalAngle * Mathf.Deg2Rad;
                        float newY = y * Mathf.Cos(tiltRad) - z * Mathf.Sin(tiltRad);
                        float newZ = y * Mathf.Sin(tiltRad) + z * Mathf.Cos(tiltRad);
                        y = newY;
                        z = newZ;
                    }
                    
                    basePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        /// <summary>
        /// Collect all bubble transforms that need positioning
        /// </summary>
        void CollectBubbleTransforms()
        {
            bubbleTransforms.Clear();
            
            // Find all bubble objects in children
            BubbleSpringPhysics[] bubbles = GetComponentsInChildren<BubbleSpringPhysics>();
            foreach (var bubble in bubbles)
            {
                bubbleTransforms.Add(bubble.transform);
            }
            
            // If we have more positions than bubbles, that's fine
            // If we have more bubbles than positions, we need to recalculate
            if (bubbleTransforms.Count > basePositions.Count)
            {
                Debug.LogWarning($"More bubbles ({bubbleTransforms.Count}) than calculated positions ({basePositions.Count}). Recalculating layout.");
                CalculateBasePositions();
            }
        }
        
        /// <summary>
        /// Update bubble positions with breathing and wave motion
        /// </summary>
        void UpdateBubblePositions()
        {
            if (userHead == null || bubbleTransforms.Count == 0)
                return;
            
            Vector3 userPosition = userHead.position;
            Quaternion userRotation = userHead.rotation;
            
            for (int i = 0; i < bubbleTransforms.Count && i < basePositions.Count; i++)
            {
                Vector3 basePos = basePositions[i];
                Vector3 finalPos = basePos;
                
                // Apply breathing animation
                if (enableBreathing)
                {
                    float breathingPhase = animationTime * breathingFrequency * 2.0f * Mathf.PI;
                    float breathingOffset = Mathf.Sin(breathingPhase + (i * 0.2f)) * 0.02f;
                    finalPos += Vector3.up * breathingOffset;
                }
                
                // Apply wave motion
                if (enableWaveMotion)
                {
                    float wavePhase = animationTime * waveMotionSpeed + (i * 0.3f);
                    float waveX = Mathf.Sin(wavePhase) * 0.05f;
                    float waveZ = Mathf.Cos(wavePhase * 0.7f) * 0.05f;
                    finalPos += new Vector3(waveX, 0, waveZ);
                }
                
                // Transform to world space relative to user
                finalPos = userPosition + (userRotation * finalPos);
                
                // Apply position smoothly
                bubbleTransforms[i].position = Vector3.Lerp(
                    bubbleTransforms[i].position, 
                    finalPos, 
                    Time.deltaTime * 5.0f
                );
                
                // Make bubbles face the user (optional)
                Vector3 lookDirection = (userPosition - bubbleTransforms[i].position).normalized;
                bubbleTransforms[i].rotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
            }
        }
        
        /// <summary>
        /// Add a new bubble to the layout system
        /// </summary>
        public void AddBubble(Transform bubbleTransform)
        {
            if (!bubbleTransforms.Contains(bubbleTransform))
            {
                bubbleTransforms.Add(bubbleTransform);
                
                // If we need more positions, recalculate
                if (bubbleTransforms.Count > basePositions.Count)
                {
                    CalculateBasePositions();
                }
                
                isDirty = true;
            }
        }
        
        /// <summary>
        /// Remove a bubble from the layout system
        /// </summary>
        public void RemoveBubble(Transform bubbleTransform)
        {
            if (bubbleTransforms.Contains(bubbleTransform))
            {
                bubbleTransforms.Remove(bubbleTransform);
                isDirty = true;
            }
        }
        
        /// <summary>
        /// Recalculate layout when parameters change
        /// </summary>
        public void RefreshLayout()
        {
            CalculateBasePositions();
            isDirty = true;
        }
        
        /// <summary>
        /// Get the closest bubble to a world position
        /// </summary>
        public Transform GetClosestBubble(Vector3 worldPosition)
        {
            Transform closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (var bubble in bubbleTransforms)
            {
                float distance = Vector3.Distance(worldPosition, bubble.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = bubble;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Get all bubbles within a radius of a world position
        /// </summary>
        public List<Transform> GetBubblesInRadius(Vector3 worldPosition, float radius)
        {
            List<Transform> bubblesInRadius = new List<Transform>();
            
            foreach (var bubble in bubbleTransforms)
            {
                float distance = Vector3.Distance(worldPosition, bubble.position);
                if (distance <= radius)
                {
                    bubblesInRadius.Add(bubble);
                }
            }
            
            return bubblesInRadius;
        }
        
        // Editor helper methods
        void OnValidate()
        {
            // Recalculate when values change in editor
            if (Application.isPlaying)
            {
                isDirty = true;
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw layout visualization in editor
            if (basePositions.Count > 0 && userHead != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 userPos = userHead.position;
                Quaternion userRot = userHead.rotation;
                
                foreach (var pos in basePositions)
                {
                    Vector3 worldPos = userPos + (userRot * pos);
                    Gizmos.DrawWireSphere(worldPos, 0.05f);
                }
                
                // Draw wave radius
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(userPos, waveRadius);
            }
        }
    }
}