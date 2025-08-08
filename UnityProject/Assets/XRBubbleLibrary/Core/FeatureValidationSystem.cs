using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Comprehensive feature validation system using reflection and runtime analysis.
    /// Provides automatic validation of feature gates and dependency checking.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class FeatureValidationSystem : MonoBehaviour
    {
        [Header("Validation Configuration")]
        [SerializeField] private bool enableAutomaticValidation = true;
        [SerializeField] private bool validateOnStart = true;
        [SerializeField] private bool logValidationResults = true;
        [SerializeField] private float validationInterval = 30f; // seconds

        [Header("Validation Scope")]
        [SerializeField] private bool validateAllAssemblies = false;
        [SerializeField] private string[] assemblyNamesToValidate = { "XRBubbleLibrary" };

        private Dictionary<ExperimentalFeature, List<FeatureGateInfo>> featureGateMap;
        private ValidationResults lastValidationResults;
        private float lastValidationTime;

        /// <summary>
        /// Event fired when validation completes.
        /// </summary>
        public event Action<ValidationResults> OnValidationComplete;

        /// <summary>
        /// Event fired when a feature gate violation is detected.
        /// </summary>
        public event Action<FeatureGateViolation> OnFeatureGateViolation;

        private void Start()
        {
            if (validateOnStart)
            {
                PerformFullValidation();
            }
        }

        private void Update()
        {
            if (enableAutomaticValidation && 
                Time.time - lastValidationTime > validationInterval)
            {
                PerformIncrementalValidation();
            }
        }

        /// <summary>
        /// Perform a comprehensive validation of all feature gates in the system.
        /// </summary>
        public ValidationResults PerformFullValidation()
        {
            var startTime = Time.realtimeSinceStartup;
            
            if (logValidationResults)
            {
                Debug.Log("[FeatureValidation] Starting full feature gate validation...");
            }

            var results = new ValidationResults
            {
                ValidationTime = DateTime.UtcNow,
                ValidationType = ValidationType.Full
            };

            try
            {
                // Discover all feature gates
                DiscoverFeatureGates();

                // Validate each feature gate
                ValidateAllFeatureGates(results);

                // Validate feature dependencies
                ValidateFeatureDependencies(results);

                // Validate assembly constraints
                ValidateAssemblyConstraints(results);

                results.Success = results.Violations.Count == 0;
                results.ValidationDuration = Time.realtimeSinceStartup - startTime;

                lastValidationResults = results;
                lastValidationTime = Time.time;

                if (logValidationResults)
                {
                    LogValidationResults(results);
                }

                OnValidationComplete?.Invoke(results);
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.ValidationError = ex.Message;
                results.ValidationDuration = Time.realtimeSinceStartup - startTime;

                Debug.LogError($"[FeatureValidation] Validation failed with exception: {ex}");
            }

            return results;
        }

        /// <summary>
        /// Perform a quick incremental validation focusing on changed features.
        /// </summary>
        public ValidationResults PerformIncrementalValidation()
        {
            var startTime = Time.realtimeSinceStartup;
            
            var results = new ValidationResults
            {
                ValidationTime = DateTime.UtcNow,
                ValidationType = ValidationType.Incremental
            };

            try
            {
                // Only validate features that have changed state
                var changedFeatures = DetectFeatureStateChanges();
                
                if (changedFeatures.Count > 0)
                {
                    ValidateChangedFeatures(changedFeatures, results);
                    
                    if (logValidationResults && results.Violations.Count > 0)
                    {
                        Debug.LogWarning($"[FeatureValidation] Incremental validation found {results.Violations.Count} violations");
                    }
                }

                results.Success = results.Violations.Count == 0;
                results.ValidationDuration = Time.realtimeSinceStartup - startTime;
                lastValidationTime = Time.time;

                if (results.Violations.Count > 0)
                {
                    OnValidationComplete?.Invoke(results);
                }
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.ValidationError = ex.Message;
                results.ValidationDuration = Time.realtimeSinceStartup - startTime;

                Debug.LogError($"[FeatureValidation] Incremental validation failed: {ex}");
            }

            return results;
        }

        /// <summary>
        /// Validate a specific experimental feature and its gates.
        /// </summary>
        public FeatureValidationResult ValidateFeature(ExperimentalFeature feature)
        {
            var result = new FeatureValidationResult
            {
                Feature = feature,
                IsEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(feature),
                ValidationTime = DateTime.UtcNow
            };

            if (featureGateMap != null && featureGateMap.ContainsKey(feature))
            {
                var gates = featureGateMap[feature];
                result.TotalGates = gates.Count;

                foreach (var gate in gates)
                {
                    try
                    {
                        gate.Attribute.ValidateFeatureAccess();
                        result.ValidGates++;
                    }
                    catch (FeatureDisabledException ex)
                    {
                        result.Violations.Add(new FeatureGateViolation
                        {
                            Feature = feature,
                            TargetName = gate.TargetName,
                            TargetType = gate.TargetType,
                            ErrorMessage = ex.Message,
                            ViolationType = ViolationType.FeatureDisabled
                        });
                    }
                    catch (Exception ex)
                    {
                        result.Violations.Add(new FeatureGateViolation
                        {
                            Feature = feature,
                            TargetName = gate.TargetName,
                            TargetType = gate.TargetType,
                            ErrorMessage = ex.Message,
                            ViolationType = ViolationType.ValidationError
                        });
                    }
                }
            }

            result.Success = result.Violations.Count == 0;
            return result;
        }

        /// <summary>
        /// Get the current validation status without performing new validation.
        /// </summary>
        public ValidationResults GetLastValidationResults()
        {
            return lastValidationResults ?? new ValidationResults
            {
                ValidationTime = DateTime.MinValue,
                Success = false,
                ValidationError = "No validation has been performed yet"
            };
        }

        /// <summary>
        /// Generate a comprehensive validation report.
        /// </summary>
        public string GenerateValidationReport()
        {
            var results = lastValidationResults ?? PerformFullValidation();
            
            var report = "# Feature Validation Report\n\n";
            report += $"**Validation Time:** {results.ValidationTime:yyyy-MM-dd HH:mm:ss} UTC\n";
            report += $"**Validation Type:** {results.ValidationType}\n";
            report += $"**Duration:** {results.ValidationDuration:F3} seconds\n";
            report += $"**Status:** {(results.Success ? "✅ PASSED" : "❌ FAILED")}\n\n";

            if (!string.IsNullOrEmpty(results.ValidationError))
            {
                report += $"**Error:** {results.ValidationError}\n\n";
            }

            if (results.Violations.Count > 0)
            {
                report += $"## Violations Found ({results.Violations.Count})\n\n";
                
                var violationsByFeature = results.Violations.GroupBy(v => v.Feature);
                
                foreach (var group in violationsByFeature)
                {
                    var feature = group.Key;
                    var violations = group.ToList();
                    
                    report += $"### {feature} ({violations.Count} violations)\n\n";
                    
                    foreach (var violation in violations)
                    {
                        var icon = violation.ViolationType switch
                        {
                            ViolationType.FeatureDisabled => "🚫",
                            ViolationType.DependencyMissing => "⚠️",
                            ViolationType.ValidationError => "❌",
                            _ => "❓"
                        };
                        
                        report += $"- {icon} **{violation.TargetName}** ({violation.TargetType})\n";
                        report += $"  - {violation.ErrorMessage}\n\n";
                    }
                }
            }
            else
            {
                report += "## ✅ No Violations Found\n\n";
                report += "All feature gates are properly configured and validated.\n\n";
            }

            // Feature summary
            if (featureGateMap != null)
            {
                report += "## Feature Gate Summary\n\n";
                
                foreach (var kvp in featureGateMap)
                {
                    var feature = kvp.Key;
                    var gates = kvp.Value;
                    var isEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(feature);
                    var statusIcon = isEnabled ? "✅" : "❌";
                    
                    report += $"- {statusIcon} **{feature}**: {gates.Count} gates\n";
                }
                
                report += "\n";
            }

            report += $"*Generated by FeatureValidationSystem at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*\n";
            return report;
        }

        private void DiscoverFeatureGates()
        {
            featureGateMap = new Dictionary<ExperimentalFeature, List<FeatureGateInfo>>();
            
            var assemblies = validateAllAssemblies ? 
                AppDomain.CurrentDomain.GetAssemblies() :
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => assemblyNamesToValidate.Any(name => a.FullName.Contains(name)));

            foreach (var assembly in assemblies)
            {
                try
                {
                    DiscoverFeatureGatesInAssembly(assembly);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[FeatureValidation] Failed to analyze assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }

        private void DiscoverFeatureGatesInAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                // Check class-level attributes
                var classAttribute = type.GetCustomAttribute<FeatureGateAttribute>();
                if (classAttribute != null)
                {
                    AddFeatureGate(classAttribute.RequiredFeature, new FeatureGateInfo
                    {
                        TargetType = FeatureGateTargetType.Class,
                        TargetName = type.FullName,
                        Attribute = classAttribute
                    });
                }

                // Check method-level attributes
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    var methodAttribute = method.GetCustomAttribute<FeatureGateAttribute>();
                    if (methodAttribute != null)
                    {
                        AddFeatureGate(methodAttribute.RequiredFeature, new FeatureGateInfo
                        {
                            TargetType = FeatureGateTargetType.Method,
                            TargetName = $"{type.FullName}.{method.Name}",
                            Attribute = methodAttribute
                        });
                    }
                }

                // Check property-level attributes
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    var propertyAttribute = property.GetCustomAttribute<FeatureGateAttribute>();
                    if (propertyAttribute != null)
                    {
                        AddFeatureGate(propertyAttribute.RequiredFeature, new FeatureGateInfo
                        {
                            TargetType = FeatureGateTargetType.Property,
                            TargetName = $"{type.FullName}.{property.Name}",
                            Attribute = propertyAttribute
                        });
                    }
                }
            }
        }

        private void AddFeatureGate(ExperimentalFeature feature, FeatureGateInfo gateInfo)
        {
            if (!featureGateMap.ContainsKey(feature))
            {
                featureGateMap[feature] = new List<FeatureGateInfo>();
            }
            featureGateMap[feature].Add(gateInfo);
        }

        private void ValidateAllFeatureGates(ValidationResults results)
        {
            if (featureGateMap == null) return;

            foreach (var kvp in featureGateMap)
            {
                var feature = kvp.Key;
                var gates = kvp.Value;
                var isEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(feature);

                foreach (var gate in gates)
                {
                    try
                    {
                        // Only validate if feature is enabled or gate requires validation in release
                        if (isEnabled || gate.Attribute.ValidateInRelease)
                        {
                            gate.Attribute.ValidateFeatureAccess();
                        }
                    }
                    catch (FeatureDisabledException ex)
                    {
                        var violation = new FeatureGateViolation
                        {
                            Feature = feature,
                            TargetName = gate.TargetName,
                            TargetType = gate.TargetType,
                            ErrorMessage = ex.Message,
                            ViolationType = ViolationType.FeatureDisabled
                        };
                        
                        results.Violations.Add(violation);
                        OnFeatureGateViolation?.Invoke(violation);
                    }
                    catch (Exception ex)
                    {
                        var violation = new FeatureGateViolation
                        {
                            Feature = feature,
                            TargetName = gate.TargetName,
                            TargetType = gate.TargetType,
                            ErrorMessage = ex.Message,
                            ViolationType = ViolationType.ValidationError
                        };
                        
                        results.Violations.Add(violation);
                        OnFeatureGateViolation?.Invoke(violation);
                    }
                }
            }
        }

        private void ValidateFeatureDependencies(ValidationResults results)
        {
            var flagManager = CompilerFlagManager.Instance;
            
            // Check AI Integration dependencies
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.CLOUD_INFERENCE) &&
                !flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                results.Violations.Add(new FeatureGateViolation
                {
                    Feature = ExperimentalFeature.CLOUD_INFERENCE,
                    TargetName = "Dependency Check",
                    TargetType = FeatureGateTargetType.Class,
                    ErrorMessage = "Cloud Inference requires AI Integration to be enabled",
                    ViolationType = ViolationType.DependencyMissing
                });
            }

            if (flagManager.IsFeatureEnabled(ExperimentalFeature.ON_DEVICE_ML) &&
                !flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                results.Violations.Add(new FeatureGateViolation
                {
                    Feature = ExperimentalFeature.ON_DEVICE_ML,
                    TargetName = "Dependency Check",
                    TargetType = FeatureGateTargetType.Class,
                    ErrorMessage = "On-Device ML requires AI Integration to be enabled",
                    ViolationType = ViolationType.DependencyMissing
                });
            }
        }

        private void ValidateAssemblyConstraints(ValidationResults results)
        {
            // Check that assemblies with defineConstraints are properly configured
            // This would require additional assembly analysis which we'll implement later
        }

        private List<ExperimentalFeature> DetectFeatureStateChanges()
        {
            var changedFeatures = new List<ExperimentalFeature>();
            
            if (lastValidationResults != null)
            {
                foreach (ExperimentalFeature feature in Enum.GetValues(typeof(ExperimentalFeature)))
                {
                    var currentState = CompilerFlagManager.Instance.IsFeatureEnabled(feature);
                    // For now, we'll assume all features might have changed
                    // In a more sophisticated implementation, we'd track previous states
                    changedFeatures.Add(feature);
                }
            }
            
            return changedFeatures;
        }

        private void ValidateChangedFeatures(List<ExperimentalFeature> changedFeatures, ValidationResults results)
        {
            foreach (var feature in changedFeatures)
            {
                var featureResult = ValidateFeature(feature);
                results.Violations.AddRange(featureResult.Violations);
            }
        }

        private void LogValidationResults(ValidationResults results)
        {
            if (results.Success)
            {
                Debug.Log($"[FeatureValidation] ✅ Validation completed successfully in {results.ValidationDuration:F3}s");
            }
            else
            {
                Debug.LogWarning($"[FeatureValidation] ❌ Validation failed with {results.Violations.Count} violations in {results.ValidationDuration:F3}s");
                
                foreach (var violation in results.Violations.Take(5)) // Log first 5 violations
                {
                    Debug.LogWarning($"[FeatureValidation] - {violation.Feature}: {violation.ErrorMessage}");
                }
                
                if (results.Violations.Count > 5)
                {
                    Debug.LogWarning($"[FeatureValidation] ... and {results.Violations.Count - 5} more violations");
                }
            }
        }
    }

    /// <summary>
    /// Results of a feature validation operation.
    /// </summary>
    [Serializable]
    public class ValidationResults
    {
        public DateTime ValidationTime;
        public ValidationType ValidationType;
        public bool Success;
        public float ValidationDuration;
        public string ValidationError;
        public List<FeatureGateViolation> Violations = new List<FeatureGateViolation>();
    }

    /// <summary>
    /// Results of validating a specific experimental feature.
    /// </summary>
    [Serializable]
    public class FeatureValidationResult
    {
        public ExperimentalFeature Feature;
        public bool IsEnabled;
        public DateTime ValidationTime;
        public bool Success;
        public int TotalGates;
        public int ValidGates;
        public List<FeatureGateViolation> Violations = new List<FeatureGateViolation>();
    }

    /// <summary>
    /// Information about a feature gate violation.
    /// </summary>
    [Serializable]
    public class FeatureGateViolation
    {
        public ExperimentalFeature Feature;
        public string TargetName;
        public FeatureGateTargetType TargetType;
        public string ErrorMessage;
        public ViolationType ViolationType;
    }

    /// <summary>
    /// Type of validation being performed.
    /// </summary>
    public enum ValidationType
    {
        Full,
        Incremental
    }

    /// <summary>
    /// Type of violation detected during validation.
    /// </summary>
    public enum ViolationType
    {
        FeatureDisabled,
        DependencyMissing,
        ValidationError
    }
}