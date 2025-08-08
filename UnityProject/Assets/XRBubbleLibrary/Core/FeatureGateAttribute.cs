using System;
using System.Reflection;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Attribute for runtime feature gating with compile-time and runtime validation.
    /// Provides additional safety layer beyond compiler flags for experimental features.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property, 
                    AllowMultiple = false, Inherited = true)]
    public class FeatureGateAttribute : Attribute
    {
        /// <summary>
        /// The experimental feature required for this functionality.
        /// </summary>
        public ExperimentalFeature RequiredFeature { get; }

        /// <summary>
        /// Optional custom error message when feature is disabled.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Whether to throw an exception or just log a warning when feature is disabled.
        /// </summary>
        public bool ThrowOnDisabled { get; set; } = true;

        /// <summary>
        /// Whether to perform validation in release builds (default: editor only).
        /// </summary>
        public bool ValidateInRelease { get; set; } = false;

        /// <summary>
        /// Initialize feature gate attribute with required feature.
        /// </summary>
        /// <param name="requiredFeature">The experimental feature required</param>
        public FeatureGateAttribute(ExperimentalFeature requiredFeature)
        {
            RequiredFeature = requiredFeature;
        }

        /// <summary>
        /// Validate that the required feature is enabled before allowing execution.
        /// Called automatically by FeatureGateValidator or manually by developers.
        /// </summary>
        public void ValidateFeatureAccess()
        {
            // Skip validation in release builds unless explicitly requested
            if (!ValidateInRelease && !Application.isEditor && !Debug.isDebugBuild)
            {
                return;
            }

            if (!CompilerFlagManager.Instance.IsFeatureEnabled(RequiredFeature))
            {
                var message = ErrorMessage ?? 
                    $"Feature {RequiredFeature} is disabled. Enable via Project Settings > XR Bubble Library > Experimental Features";

                if (ThrowOnDisabled)
                {
                    throw new FeatureDisabledException(RequiredFeature, message);
                }
                else
                {
                    Debug.LogWarning($"[FeatureGate] {message}");
                }
            }
        }

        /// <summary>
        /// Get a human-readable description of this feature gate.
        /// </summary>
        public string GetDescription()
        {
            return $"Requires {RequiredFeature}" + 
                   (ThrowOnDisabled ? " (throws on disabled)" : " (warns on disabled)") +
                   (ValidateInRelease ? " (validates in release)" : " (editor/debug only)");
        }
    }

    /// <summary>
    /// Exception thrown when attempting to access a disabled experimental feature.
    /// </summary>
    public class FeatureDisabledException : InvalidOperationException
    {
        /// <summary>
        /// The experimental feature that was disabled.
        /// </summary>
        public ExperimentalFeature DisabledFeature { get; }

        /// <summary>
        /// Initialize exception with disabled feature information.
        /// </summary>
        public FeatureDisabledException(ExperimentalFeature disabledFeature, string message) 
            : base(message)
        {
            DisabledFeature = disabledFeature;
        }

        /// <summary>
        /// Initialize exception with disabled feature and inner exception.
        /// </summary>
        public FeatureDisabledException(ExperimentalFeature disabledFeature, string message, Exception innerException) 
            : base(message, innerException)
        {
            DisabledFeature = disabledFeature;
        }
    }

    /// <summary>
    /// Utility class for validating feature gates through reflection.
    /// Provides automatic validation of attributed methods and classes.
    /// </summary>
    public static class FeatureGateValidator
    {
        /// <summary>
        /// Validate feature access for a specific method before execution.
        /// </summary>
        /// <param name="method">The method to validate</param>
        public static void ValidateMethod(MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<FeatureGateAttribute>();
            if (attribute != null)
            {
                attribute.ValidateFeatureAccess();
            }

            // Also check the declaring class
            var classAttribute = method.DeclaringType?.GetCustomAttribute<FeatureGateAttribute>();
            if (classAttribute != null)
            {
                classAttribute.ValidateFeatureAccess();
            }
        }

        /// <summary>
        /// Validate feature access for a specific type before instantiation.
        /// </summary>
        /// <param name="type">The type to validate</param>
        public static void ValidateType(Type type)
        {
            var attribute = type.GetCustomAttribute<FeatureGateAttribute>();
            if (attribute != null)
            {
                attribute.ValidateFeatureAccess();
            }
        }

        /// <summary>
        /// Validate feature access for a property before access.
        /// </summary>
        /// <param name="property">The property to validate</param>
        public static void ValidateProperty(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<FeatureGateAttribute>();
            if (attribute != null)
            {
                attribute.ValidateFeatureAccess();
            }

            // Also check the declaring class
            var classAttribute = property.DeclaringType?.GetCustomAttribute<FeatureGateAttribute>();
            if (classAttribute != null)
            {
                classAttribute.ValidateFeatureAccess();
            }
        }

        /// <summary>
        /// Get all feature gates defined in the current assembly.
        /// Useful for documentation and validation reporting.
        /// </summary>
        public static FeatureGateInfo[] GetAllFeatureGates()
        {
            var gates = new System.Collections.Generic.List<FeatureGateInfo>();
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                // Check class-level attributes
                var classAttribute = type.GetCustomAttribute<FeatureGateAttribute>();
                if (classAttribute != null)
                {
                    gates.Add(new FeatureGateInfo
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
                        gates.Add(new FeatureGateInfo
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
                        gates.Add(new FeatureGateInfo
                        {
                            TargetType = FeatureGateTargetType.Property,
                            TargetName = $"{type.FullName}.{property.Name}",
                            Attribute = propertyAttribute
                        });
                    }
                }
            }

            return gates.ToArray();
        }

        /// <summary>
        /// Generate a comprehensive report of all feature gates in the system.
        /// </summary>
        public static string GenerateFeatureGateReport()
        {
            var gates = GetAllFeatureGates();
            var report = "# Feature Gate Analysis Report\n\n";
            
            report += $"**Total Feature Gates Found:** {gates.Length}\n\n";

            // Group by feature
            var featureGroups = new System.Collections.Generic.Dictionary<ExperimentalFeature, System.Collections.Generic.List<FeatureGateInfo>>();
            
            foreach (var gate in gates)
            {
                if (!featureGroups.ContainsKey(gate.Attribute.RequiredFeature))
                {
                    featureGroups[gate.Attribute.RequiredFeature] = new System.Collections.Generic.List<FeatureGateInfo>();
                }
                featureGroups[gate.Attribute.RequiredFeature].Add(gate);
            }

            foreach (var kvp in featureGroups)
            {
                var feature = kvp.Key;
                var gateList = kvp.Value;
                var isEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(feature);
                var statusIcon = isEnabled ? "✅" : "❌";

                report += $"## {statusIcon} {feature} ({gateList.Count} gates)\n\n";

                foreach (var gate in gateList)
                {
                    var targetIcon = gate.TargetType switch
                    {
                        FeatureGateTargetType.Class => "🏛️",
                        FeatureGateTargetType.Method => "⚙️",
                        FeatureGateTargetType.Property => "📋",
                        _ => "❓"
                    };

                    report += $"- {targetIcon} **{gate.TargetName}**\n";
                    report += $"  - {gate.Attribute.GetDescription()}\n";
                    
                    if (!string.IsNullOrEmpty(gate.Attribute.ErrorMessage))
                    {
                        report += $"  - Custom message: \"{gate.Attribute.ErrorMessage}\"\n";
                    }
                    
                    report += "\n";
                }
            }

            report += $"\n*Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*\n";
            return report;
        }
    }

    /// <summary>
    /// Information about a feature gate found through reflection.
    /// </summary>
    public class FeatureGateInfo
    {
        public FeatureGateTargetType TargetType { get; set; }
        public string TargetName { get; set; }
        public FeatureGateAttribute Attribute { get; set; }
    }

    /// <summary>
    /// Type of target that has a feature gate attribute.
    /// </summary>
    public enum FeatureGateTargetType
    {
        Class,
        Method,
        Property
    }

    /// <summary>
    /// Extension methods for easier feature gate validation.
    /// </summary>
    public static class FeatureGateExtensions
    {
        /// <summary>
        /// Validate feature gates for the current method.
        /// Call this at the beginning of methods that require feature validation.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void ValidateFeatureGates()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            
            if (method != null && method is MethodInfo methodInfo)
            {
                FeatureGateValidator.ValidateMethod(methodInfo);
            }
        }

        /// <summary>
        /// Check if a type has any feature gate restrictions.
        /// </summary>
        public static bool HasFeatureGate(this Type type)
        {
            return type.GetCustomAttribute<FeatureGateAttribute>() != null;
        }

        /// <summary>
        /// Check if a method has any feature gate restrictions.
        /// </summary>
        public static bool HasFeatureGate(this MethodInfo method)
        {
            return method.GetCustomAttribute<FeatureGateAttribute>() != null ||
                   method.DeclaringType?.HasFeatureGate() == true;
        }

        /// <summary>
        /// Get the required feature for a type, if any.
        /// </summary>
        public static ExperimentalFeature? GetRequiredFeature(this Type type)
        {
            var attribute = type.GetCustomAttribute<FeatureGateAttribute>();
            return attribute?.RequiredFeature;
        }

        /// <summary>
        /// Get the required feature for a method, if any.
        /// </summary>
        public static ExperimentalFeature? GetRequiredFeature(this MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<FeatureGateAttribute>();
            if (attribute != null)
            {
                return attribute.RequiredFeature;
            }

            // Check declaring class
            return method.DeclaringType?.GetRequiredFeature();
        }
    }
}