using UnityEngine;
using Unity.Mathematics;
using XRBubbleLibrary.WaveMatrix;
using TMPro;

namespace XRBubbleLibrary.WordBubbles
{
    /// <summary>
    /// Factory for creating and managing word bubbles
    /// Handles bubble creation, positioning, and lifecycle management
    /// </summary>
    public class WordBubbleFactory : MonoBehaviour
    {
        [Header("Bubble Creation")]
        [SerializeField] private GameObject bubblePrefab;
        [SerializeField] private BubbleVisualSettings defaultVisualSettings = BubbleVisualSettings.Default;
        
        [Header("Positioning")]
        [SerializeField] private WaveMatrixPositioner wavePositioner;
        [SerializeField] private bool autoPosition = true;
        
        [Header("Performance")]
        [SerializeField] private int maxActiveBubbles = 30; // Reduced for Quest 3
        [SerializeField] private bool useObjectPooling = true;
        [SerializeField] private float lodDistance = 5f; // Hide bubbles beyond this distance
        [SerializeField] private float textLodDistance = 3f; // Hide text beyond this distance
        
        // Bubble management
        private System.Collections.Generic.List<WordBubble> activeBubbles = new System.Collections.Generic.List<WordBubble>();
        private System.Collections.Generic.Queue<GameObject> bubblePool = new System.Collections.Generic.Queue<GameObject>();
        
        // Events
        public System.Action<WordBubble> OnBubbleCreated;
        public System.Action<WordBubble> OnBubbleDestroyed;
        
        void Start()
        {
            InitializeFactory();
        }
        
        void InitializeFactory()
        {
            // Get wave positioner if not assigned
            if (wavePositioner == null)
                wavePositioner = FindObjectOfType<WaveMatrixPositioner>();
            
            // Create bubble prefab if none provided
            if (bubblePrefab == null)
            {
                bubblePrefab = CreateDefaultBubblePrefab();
            }
            
            // Pre-populate object pool
            if (useObjectPooling)
            {
                PrePopulatePool();
            }
            
            Debug.Log($"WordBubbleFactory initialized: Max bubbles={maxActiveBubbles}, Pooling={useObjectPooling}");
        }
        
        GameObject CreateDefaultBubblePrefab()
        {
            // Create default bubble prefab
            GameObject prefab = new GameObject("DefaultWordBubble");
            
            // Add sphere mesh
            var meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateSphereMesh();
            
            // Add renderer
            var renderer = prefab.AddComponent<MeshRenderer>();
            
            // Add collider
            var collider = prefab.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            
            // Add WordBubble component
            prefab.AddComponent<WordBubble>();
            
            // Create text child object
            GameObject textObject = new GameObject("BubbleText");
            textObject.transform.parent = prefab.transform;
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;
            
            // Add TextMeshPro component
            var textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.text = "Word";
            textMesh.fontSize = 0.5f;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableAutoSizing = true;
            
            return prefab;
        }
        
        Mesh CreateSphereMesh()
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = sphere.GetComponent<MeshFilter>().mesh;
            DestroyImmediate(sphere);
            return mesh;
        }
        
        void PrePopulatePool()
        {
            int poolSize = Mathf.Min(maxActiveBubbles / 2, 25);
            
            for (int i = 0; i < poolSize; i++)
            {
                GameObject bubble = Instantiate(bubblePrefab);
                bubble.SetActive(false);
                bubble.transform.parent = transform;
                bubblePool.Enqueue(bubble);
            }
            
            Debug.Log($"Bubble pool pre-populated with {poolSize} bubbles");
        }
        
        /// <summary>
        /// Create a new word bubble with specified text
        /// </summary>
        public WordBubble CreateWordBubble(string word, float aiConfidence = 0.5f)
        {
            if (activeBubbles.Count >= maxActiveBubbles)
            {
                Debug.LogWarning($"Maximum active bubbles ({maxActiveBubbles}) reached");
                return null;
            }
            
            GameObject bubbleObject = GetBubbleFromPool();
            WordBubble bubble = bubbleObject.GetComponent<WordBubble>();
            
            if (bubble == null)
            {
                bubble = bubbleObject.AddComponent<WordBubble>();
            }
            
            // Configure bubble
            bubble.Word = word;
            bubble.AIConfidence = aiConfidence;
            bubble.UpdateVisualSettings(defaultVisualSettings);
            
            // Position bubble if auto-positioning is enabled
            if (autoPosition && wavePositioner != null)
            {
                int bubbleIndex = activeBubbles.Count;
                float3 position = wavePositioner.GetRealTimePosition(bubbleIndex, CalculateAIDistance(aiConfidence));
                bubbleObject.transform.localPosition = position;
            }
            
            // Activate and register
            bubbleObject.SetActive(true);
            activeBubbles.Add(bubble);
            
            // Setup event handlers
            bubble.OnBubbleSelected += HandleBubbleSelected;
            bubble.OnBubbleHovered += HandleBubbleHovered;
            bubble.OnBubbleUnhovered += HandleBubbleUnhovered;
            
            OnBubbleCreated?.Invoke(bubble);
            
            Debug.Log($"Created word bubble: '{word}' (confidence: {aiConfidence:F2})");
            return bubble;
        }
        
        /// <summary>
        /// Create multiple word bubbles from an array of words
        /// </summary>
        public WordBubble[] CreateWordBubbles(string[] words, float[] confidences = null)
        {
            if (words == null || words.Length == 0)
                return new WordBubble[0];
            
            var bubbles = new WordBubble[words.Length];
            
            for (int i = 0; i < words.Length; i++)
            {
                float confidence = (confidences != null && i < confidences.Length) ? confidences[i] : 0.5f;
                bubbles[i] = CreateWordBubble(words[i], confidence);
            }
            
            return bubbles;
        }
        
        /// <summary>
        /// Destroy a word bubble and return it to pool
        /// </summary>
        public void DestroyWordBubble(WordBubble bubble)
        {
            if (bubble == null || !activeBubbles.Contains(bubble))
                return;
            
            // Remove event handlers
            bubble.OnBubbleSelected -= HandleBubbleSelected;
            bubble.OnBubbleHovered -= HandleBubbleHovered;
            bubble.OnBubbleUnhovered -= HandleBubbleUnhovered;
            
            // Remove from active list
            activeBubbles.Remove(bubble);
            
            OnBubbleDestroyed?.Invoke(bubble);
            
            // Return to pool or destroy
            ReturnBubbleToPool(bubble.gameObject);
        }
        
        /// <summary>
        /// Clear all active bubbles
        /// </summary>
        public void ClearAllBubbles()
        {
            var bubblesToDestroy = new WordBubble[activeBubbles.Count];
            activeBubbles.CopyTo(bubblesToDestroy);
            
            foreach (var bubble in bubblesToDestroy)
            {
                DestroyWordBubble(bubble);
            }
            
            Debug.Log("All word bubbles cleared");
        }
        
        /// <summary>
        /// Update positions of all active bubbles
        /// </summary>
        public void UpdateBubblePositions()
        {
            if (wavePositioner == null) return;
            
            for (int i = 0; i < activeBubbles.Count; i++)
            {
                var bubble = activeBubbles[i];
                if (bubble != null)
                {
                    float aiDistance = CalculateAIDistance(bubble.AIConfidence);
                    float3 position = wavePositioner.GetRealTimePosition(i, aiDistance);
                    bubble.transform.localPosition = position;
                }
            }
        }
        
        /// <summary>
        /// Update visual settings for all active bubbles
        /// </summary>
        public void UpdateAllBubbleVisuals(BubbleVisualSettings newSettings)
        {
            defaultVisualSettings = newSettings;
            
            foreach (var bubble in activeBubbles)
            {
                if (bubble != null)
                {
                    bubble.UpdateVisualSettings(newSettings);
                }
            }
        }
        
        #region Object Pooling
        
        GameObject GetBubbleFromPool()
        {
            if (useObjectPooling && bubblePool.Count > 0)
            {
                return bubblePool.Dequeue();
            }
            else
            {
                GameObject bubble = Instantiate(bubblePrefab, transform);
                return bubble;
            }
        }
        
        void ReturnBubbleToPool(GameObject bubble)
        {
            if (useObjectPooling)
            {
                bubble.SetActive(false);
                bubble.transform.parent = transform;
                bubblePool.Enqueue(bubble);
            }
            else
            {
                Destroy(bubble);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        void HandleBubbleSelected(WordBubble bubble)
        {
            Debug.Log($"Bubble selected: '{bubble.Word}'");
            // Additional selection logic can be added here
        }
        
        void HandleBubbleHovered(WordBubble bubble)
        {
            Debug.Log($"Bubble hovered: '{bubble.Word}'");
            // Additional hover logic can be added here
        }
        
        void HandleBubbleUnhovered(WordBubble bubble)
        {
            Debug.Log($"Bubble unhovered: '{bubble.Word}'");
            // Additional unhover logic can be added here
        }
        
        #endregion
        
        #region Utility Methods
        
        float CalculateAIDistance(float confidence)
        {
            // Higher confidence = closer to user (smaller distance)
            // Map confidence [0,1] to distance [2, -1] (negative means closer)
            return Mathf.Lerp(2f, -1f, confidence);
        }
        
        /// <summary>
        /// Get all active word bubbles
        /// </summary>
        public WordBubble[] GetActiveBubbles()
        {
            return activeBubbles.ToArray();
        }
        
        /// <summary>
        /// Get bubble count
        /// </summary>
        public int GetActiveBubbleCount()
        {
            return activeBubbles.Count;
        }
        
        /// <summary>
        /// Find bubble by word
        /// </summary>
        public WordBubble FindBubbleByWord(string word)
        {
            return activeBubbles.Find(b => b.Word.Equals(word, System.StringComparison.OrdinalIgnoreCase));
        }
        
        #endregion
        
        void Update()
        {
            // Update bubble positions if auto-positioning is enabled
            if (autoPosition)
            {
                UpdateBubblePositions();
            }
            
            // Apply LOD system for performance
            ApplyLODSystem();
        }
        
        void ApplyLODSystem()
        {
            if (Camera.main == null) return;
            
            Vector3 cameraPos = Camera.main.transform.position;
            
            foreach (var bubble in activeBubbles)
            {
                if (bubble == null) continue;
                
                float distance = Vector3.Distance(bubble.transform.position, cameraPos);
                
                // Hide entire bubble if too far
                if (distance > lodDistance)
                {
                    bubble.gameObject.SetActive(false);
                }
                else
                {
                    bubble.gameObject.SetActive(true);
                    
                    // Hide text if at medium distance
                    var textComponent = bubble.GetComponentInChildren<TextMeshPro>();
                    if (textComponent != null)
                    {
                        textComponent.enabled = distance <= textLodDistance;
                    }
                }
            }
        }
        
        // Debug info
        void OnGUI()
        {
            if (Application.isPlaying)
            {
                GUI.Label(new Rect(10, 10, 200, 20), $"Active Bubbles: {activeBubbles.Count}/{maxActiveBubbles}");
                GUI.Label(new Rect(10, 30, 200, 20), $"Pool Size: {bubblePool.Count}");
            }
        }
    }
}