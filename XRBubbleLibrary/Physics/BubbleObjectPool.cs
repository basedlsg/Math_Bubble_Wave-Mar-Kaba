using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace XRBubbleLibrary.Physics
{
    /// <summary>
    /// Object pooling system for bubble physics optimization
    /// Cloned from Unity object pooling samples, optimized for Quest 3 performance
    /// </summary>
    public class BubbleObjectPool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        public GameObject bubblePrefab;
        
        [Range(10, 200)]
        public int initialPoolSize = 50;
        
        [Range(5, 100)]
        public int maxPoolSize = 100;
        
        [Range(1, 20)]
        public int expandPoolBy = 10;
        
        [Header("Performance Settings")]
        public bool enableAutoCleanup = true;
        
        [Range(5.0f, 60.0f)]
        public float cleanupInterval = 30.0f;
        
        [Range(0.5f, 10.0f)]
        public float maxInactiveTime = 5.0f;
        
        [Header("Quest 3 Optimization")]
        public bool enableLODPooling = true;
        public int highLODPoolSize = 20;
        public int mediumLODPoolSize = 30;
        public int lowLODPoolSize = 50;
        
        // Pool management
        private Queue<PooledBubble> availableBubbles = new Queue<PooledBubble>();
        private List<PooledBubble> activeBubbles = new List<PooledBubble>();
        private Dictionary<int, PooledBubble> bubbleRegistry = new Dictionary<int, PooledBubble>();
        
        // LOD pools for performance optimization
        private Queue<PooledBubble> highLODPool = new Queue<PooledBubble>();
        private Queue<PooledBubble> mediumLODPool = new Queue<PooledBubble>();
        private Queue<PooledBubble> lowLODPool = new Queue<PooledBubble>();
        
        // Performance tracking
        private int nextBubbleId = 0;
        private float lastCleanupTime = 0.0f;
        private int totalBubblesCreated = 0;
        private int totalBubblesDestroyed = 0;
        
        // Singleton pattern for easy access
        private static BubbleObjectPool instance;
        public static BubbleObjectPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BubbleObjectPool>();
                    if (instance == null)
                    {
                        GameObject poolObject = new GameObject("BubbleObjectPool");
                        instance = poolObject.AddComponent<BubbleObjectPool>();
                    }
                }
                return instance;
            }
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializePool();
        }
        
        void Update()
        {
            if (enableAutoCleanup && Time.time - lastCleanupTime >= cleanupInterval)
            {
                CleanupInactiveBubbles();
                lastCleanupTime = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize the object pool
        /// Based on cloned Unity pooling samples
        /// </summary>
        void InitializePool()
        {
            if (bubblePrefab == null)
            {
                Debug.LogError("Bubble prefab not assigned to BubbleObjectPool!");
                return;
            }
            
            // Create initial pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledBubble();
            }
            
            // Create LOD-specific pools if enabled
            if (enableLODPooling)
            {
                CreateLODPools();
            }
            
            Debug.Log($"BubbleObjectPool initialized with {initialPoolSize} bubbles");
        }
        
        /// <summary>
        /// Create LOD-specific pools for performance optimization
        /// </summary>
        void CreateLODPools()
        {
            // High LOD pool (close bubbles, full quality)
            for (int i = 0; i < highLODPoolSize; i++)
            {
                PooledBubble bubble = CreatePooledBubble();
                bubble.SetLODLevel(0);
                highLODPool.Enqueue(bubble);
            }
            
            // Medium LOD pool (medium distance, reduced quality)
            for (int i = 0; i < mediumLODPoolSize; i++)
            {
                PooledBubble bubble = CreatePooledBubble();
                bubble.SetLODLevel(1);
                mediumLODPool.Enqueue(bubble);
            }
            
            // Low LOD pool (far distance, minimal quality)
            for (int i = 0; i < lowLODPoolSize; i++)
            {
                PooledBubble bubble = CreatePooledBubble();
                bubble.SetLODLevel(2);
                lowLODPool.Enqueue(bubble);
            }
        }
        
        /// <summary>
        /// Create a new pooled bubble instance
        /// </summary>
        PooledBubble CreatePooledBubble()
        {
            GameObject bubbleObject = Instantiate(bubblePrefab, transform);
            bubbleObject.SetActive(false);
            
            PooledBubble pooledBubble = bubbleObject.GetComponent<PooledBubble>();
            if (pooledBubble == null)
            {
                pooledBubble = bubbleObject.AddComponent<PooledBubble>();
            }
            
            pooledBubble.Initialize(this, nextBubbleId++);
            availableBubbles.Enqueue(pooledBubble);
            totalBubblesCreated++;
            
            return pooledBubble;
        }
        
        /// <summary>
        /// Get a bubble from the pool
        /// </summary>
        public PooledBubble GetBubble(Vector3 position, int lodLevel = 0)
        {
            PooledBubble bubble = null;
            
            // Try to get from appropriate LOD pool first
            if (enableLODPooling)
            {
                bubble = GetBubbleFromLODPool(lodLevel);
            }
            
            // Fallback to general pool
            if (bubble == null)
            {
                if (availableBubbles.Count > 0)
                {
                    bubble = availableBubbles.Dequeue();
                }
                else if (activeBubbles.Count < maxPoolSize)
                {
                    bubble = CreatePooledBubble();
                    availableBubbles.Dequeue(); // Remove from available since we're using it
                }
                else
                {
                    // Pool is full, reuse oldest active bubble
                    bubble = GetOldestActiveBubble();
                    if (bubble != null)
                    {
                        ReturnBubble(bubble);
                        bubble = availableBubbles.Dequeue();
                    }
                }
            }
            
            if (bubble != null)
            {
                // Configure and activate bubble
                bubble.gameObject.SetActive(true);
                bubble.transform.position = position;
                bubble.SetLODLevel(lodLevel);
                bubble.OnGetFromPool();
                
                // Add to active tracking
                activeBubbles.Add(bubble);
                bubbleRegistry[bubble.GetId()] = bubble;
            }
            
            return bubble;
        }
        
        /// <summary>
        /// Get bubble from appropriate LOD pool
        /// </summary>
        PooledBubble GetBubbleFromLODPool(int lodLevel)
        {
            Queue<PooledBubble> targetPool = null;
            
            switch (lodLevel)
            {
                case 0:
                    targetPool = highLODPool;
                    break;
                case 1:
                    targetPool = mediumLODPool;
                    break;
                case 2:
                    targetPool = lowLODPool;
                    break;
            }
            
            if (targetPool != null && targetPool.Count > 0)
            {
                return targetPool.Dequeue();
            }
            
            return null;
        }
        
        /// <summary>
        /// Return a bubble to the pool
        /// </summary>
        public void ReturnBubble(PooledBubble bubble)
        {
            if (bubble == null) return;
            
            // Remove from active tracking
            activeBubbles.Remove(bubble);
            bubbleRegistry.Remove(bubble.GetId());
            
            // Reset bubble state
            bubble.OnReturnToPool();
            bubble.gameObject.SetActive(false);
            
            // Return to appropriate pool
            if (enableLODPooling)
            {
                ReturnBubbleToLODPool(bubble);
            }
            else
            {
                availableBubbles.Enqueue(bubble);
            }
        }
        
        /// <summary>
        /// Return bubble to appropriate LOD pool
        /// </summary>
        void ReturnBubbleToLODPool(PooledBubble bubble)
        {
            int lodLevel = bubble.GetLODLevel();
            
            switch (lodLevel)
            {
                case 0:
                    if (highLODPool.Count < highLODPoolSize)
                        highLODPool.Enqueue(bubble);
                    else
                        availableBubbles.Enqueue(bubble);
                    break;
                case 1:
                    if (mediumLODPool.Count < mediumLODPoolSize)
                        mediumLODPool.Enqueue(bubble);
                    else
                        availableBubbles.Enqueue(bubble);
                    break;
                case 2:
                    if (lowLODPool.Count < lowLODPoolSize)
                        lowLODPool.Enqueue(bubble);
                    else
                        availableBubbles.Enqueue(bubble);
                    break;
                default:
                    availableBubbles.Enqueue(bubble);
                    break;
            }
        }
        
        /// <summary>
        /// Get bubble by ID
        /// </summary>
        public PooledBubble GetBubbleById(int id)
        {
            bubbleRegistry.TryGetValue(id, out PooledBubble bubble);
            return bubble;
        }
        
        /// <summary>
        /// Get oldest active bubble for reuse
        /// </summary>
        PooledBubble GetOldestActiveBubble()
        {
            PooledBubble oldest = null;
            float oldestTime = float.MaxValue;
            
            foreach (var bubble in activeBubbles)
            {
                float activeTime = bubble.GetActiveTime();
                if (activeTime < oldestTime)
                {
                    oldestTime = activeTime;
                    oldest = bubble;
                }
            }
            
            return oldest;
        }
        
        /// <summary>
        /// Clean up inactive bubbles for performance
        /// </summary>
        void CleanupInactiveBubbles()
        {
            List<PooledBubble> bubblestoRemove = new List<PooledBubble>();
            
            foreach (var bubble in activeBubbles)
            {
                if (bubble.GetInactiveTime() > maxInactiveTime)
                {
                    bubblestoRemove.Add(bubble);
                }
            }
            
            foreach (var bubble in bubblestoRemove)
            {
                ReturnBubble(bubble);
            }
            
            if (bubblestoRemove.Count > 0)
            {
                Debug.Log($"Cleaned up {bubblestoRemove.Count} inactive bubbles");
            }
        }
        
        /// <summary>
        /// Get pool statistics for debugging
        /// </summary>
        public PoolStatistics GetStatistics()
        {
            return new PoolStatistics
            {
                totalBubblesCreated = totalBubblesCreated,
                totalBubblesDestroyed = totalBubblesDestroyed,
                activeBubbles = activeBubbles.Count,
                availableBubbles = availableBubbles.Count,
                highLODBubbles = highLODPool.Count,
                mediumLODBubbles = mediumLODPool.Count,
                lowLODBubbles = lowLODPool.Count,
                poolEfficiency = activeBubbles.Count > 0 ? (float)availableBubbles.Count / activeBubbles.Count : 1.0f
            };
        }
        
        /// <summary>
        /// Clear all bubbles and reset pool
        /// </summary>
        public void ClearPool()
        {
            // Return all active bubbles
            List<PooledBubble> activeCopy = new List<PooledBubble>(activeBubbles);
            foreach (var bubble in activeCopy)
            {
                ReturnBubble(bubble);
            }
            
            // Clear all pools
            availableBubbles.Clear();
            highLODPool.Clear();
            mediumLODPool.Clear();
            lowLODPool.Clear();
            bubbleRegistry.Clear();
            
            // Destroy all bubble objects
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
                totalBubblesDestroyed++;
            }
            
            // Reinitialize
            nextBubbleId = 0;
            InitializePool();
        }
        
        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw pool statistics
            if (Application.isPlaying)
            {
                var stats = GetStatistics();
                
                // Draw active bubbles in green
                Gizmos.color = Color.green;
                foreach (var bubble in activeBubbles)
                {
                    if (bubble != null)
                    {
                        Gizmos.DrawWireSphere(bubble.transform.position, 0.05f);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Pool statistics for debugging and optimization
    /// </summary>
    [System.Serializable]
    public struct PoolStatistics
    {
        public int totalBubblesCreated;
        public int totalBubblesDestroyed;
        public int activeBubbles;
        public int availableBubbles;
        public int highLODBubbles;
        public int mediumLODBubbles;
        public int lowLODBubbles;
        public float poolEfficiency;
        
        public override string ToString()
        {
            return $"Pool Stats - Active: {activeBubbles}, Available: {availableBubbles}, " +
                   $"Created: {totalBubblesCreated}, Destroyed: {totalBubblesDestroyed}, " +
                   $"Efficiency: {poolEfficiency:F2}";
        }
    }
}