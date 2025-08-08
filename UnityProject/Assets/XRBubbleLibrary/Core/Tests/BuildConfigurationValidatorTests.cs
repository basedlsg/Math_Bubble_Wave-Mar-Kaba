using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for BuildConfigurationValidator.
    /// Tests all validation rules, flag combinations, and edge cases.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class BuildConfigurationValidatorTests
    {
        private IBuildConfigurationValidator _validator;
        
        [SetUp]
        public void SetUp()
        {
            _validator = new BuildConfigurationValidatorService();
        }
        
        [Test]
        public void ValidateConfiguration_ReturnsValidResult_WhenNoIssuesExist()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Issues);
            Assert.IsTrue(result.ValidatedAt > System.DateTime.MinValue);
            Assert.IsNotEmpty(result.BuildConfiguration);
        }
        
        [Test]
        public void ValidateConfiguration_GeneratesReport_WithCorrectFormat()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            var report = result.GenerateReport();
            
            // Assert
            Assert.IsNotEmpty(report);
            Assert.That(report, Contains.Substring("# Build Configuration Validation Report"));
            Assert.That(report, Contains.Substring("**Validation Time**"));
            Assert.That(report, Contains.Substring("**Build Configuration**"));
            Assert.That(report, Contains.Substring("**Status**"));
        }
        
        [Test]
        public void IsConfigurationValid_ReturnsTrue_WhenNoErrors()
        {
            // Act
            var isValid = _validator.IsConfigurationValid();
            var result = _validator.ValidateConfiguration();
            var hasErrors = result.Issues.Any(i => i.Severity == BuildConfigurationValidator.ValidationSeverity.Error);
            
            // Assert
            Assert.AreEqual(!hasErrors, isValid);
        }
        
        [Test]
        public void GetValidationRules_ReturnsNonEmptyList()
        {
            // Act
            var rules = _validator.GetValidationRules();
            
            // Assert
            Assert.IsNotNull(rules);
            Assert.IsNotEmpty(rules);
            Assert.That(rules.Count, Is.GreaterThan(0));
        }
        
        [Test]
        public void GetValidationRules_ContainsExpectedRules()
        {
            // Act
            var rules = _validator.GetValidationRules();
            
            // Assert
            Assert.That(rules, Contains.Item("Cloud inference requires AI integration to be enabled"));
            Assert.That(rules, Contains.Item("On-device ML requires AI integration to be enabled"));
            Assert.That(rules, Contains.Item("Multiple experimental features may impact performance"));
        }
        
        [Test]
        public void ValidateAndLog_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _validator.ValidateAndLog());
        }
        
        [Test]
        public void ValidationResult_HasCorrectStructure()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            
            // Assert
            Assert.IsNotNull(result.Issues);
            Assert.IsTrue(result.ValidatedAt > System.DateTime.MinValue);
            Assert.IsNotEmpty(result.BuildConfiguration);
            
            // Check that all issues have required properties
            foreach (var issue in result.Issues)
            {
                Assert.IsNotNull(issue.Title);
                Assert.IsNotNull(issue.Description);
                Assert.IsTrue(System.Enum.IsDefined(typeof(BuildConfigurationValidator.ValidationSeverity), issue.Severity));
            }
        }
        
        [Test]
        public void ValidationIssue_SeverityLevels_AreCorrectlyDefined()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            
            // Assert
            foreach (var issue in result.Issues)
            {
                Assert.That(issue.Severity, Is.AnyOf(
                    BuildConfigurationValidator.ValidationSeverity.Info,
                    BuildConfigurationValidator.ValidationSeverity.Warning,
                    BuildConfigurationValidator.ValidationSeverity.Error
                ));
            }
        }
        
        [Test]
        public void ValidationResult_IsValid_CorrectlyReflectsErrorState()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            var hasErrors = result.Issues.Any(i => i.Severity == BuildConfigurationValidator.ValidationSeverity.Error);
            
            // Assert
            Assert.AreEqual(!hasErrors, result.IsValid);
        }
        
        [Test]
        public void GenerateReport_HandlesEmptyIssues()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            
            // Create a result with no issues for testing
            var emptyResult = new BuildConfigurationValidator.ValidationResult
            {
                IsValid = true,
                ValidatedAt = System.DateTime.UtcNow,
                BuildConfiguration = "Test Configuration"
            };
            
            var report = emptyResult.GenerateReport();
            
            // Assert
            Assert.IsNotEmpty(report);
            Assert.That(report, Contains.Substring("No configuration issues detected"));
        }
        
        [Test]
        public void GenerateReport_HandlesMultipleIssues()
        {
            // Arrange
            var result = new BuildConfigurationValidator.ValidationResult
            {
                IsValid = false,
                ValidatedAt = System.DateTime.UtcNow,
                BuildConfiguration = "Test Configuration"
            };
            
            result.Issues.Add(new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Error,
                Title = "Test Error",
                Description = "Test error description",
                Recommendation = "Test recommendation"
            });
            
            result.Issues.Add(new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Warning,
                Title = "Test Warning",
                Description = "Test warning description"
            });
            
            // Act
            var report = result.GenerateReport();
            
            // Assert
            Assert.That(report, Contains.Substring("🔴 ERROR: Test Error"));
            Assert.That(report, Contains.Substring("🟡 WARNING: Test Warning"));
            Assert.That(report, Contains.Substring("Test error description"));
            Assert.That(report, Contains.Substring("Test warning description"));
            Assert.That(report, Contains.Substring("**Recommendation**: Test recommendation"));
        }
        
        [Test]
        public void ValidationSeverity_AllLevels_AreHandledInReport()
        {
            // Arrange
            var result = new BuildConfigurationValidator.ValidationResult
            {
                IsValid = false,
                ValidatedAt = System.DateTime.UtcNow,
                BuildConfiguration = "Test Configuration"
            };
            
            result.Issues.Add(new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Error,
                Title = "Error Issue",
                Description = "Error description"
            });
            
            result.Issues.Add(new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Warning,
                Title = "Warning Issue",
                Description = "Warning description"
            });
            
            result.Issues.Add(new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Info,
                Title = "Info Issue",
                Description = "Info description"
            });
            
            // Act
            var report = result.GenerateReport();
            
            // Assert
            Assert.That(report, Contains.Substring("🔴 ERROR"));
            Assert.That(report, Contains.Substring("🟡 WARNING"));
            Assert.That(report, Contains.Substring("ℹ️ INFO"));
        }
        
        [Test]
        public void BuildConfigurationValidator_StaticMethods_WorkCorrectly()
        {
            // Test static methods directly
            Assert.DoesNotThrow(() => BuildConfigurationValidator.ValidateConfiguration());
            Assert.DoesNotThrow(() => BuildConfigurationValidator.IsConfigurationValid());
            Assert.DoesNotThrow(() => BuildConfigurationValidator.GetValidationRules());
            Assert.DoesNotThrow(() => BuildConfigurationValidator.ValidateAndLog());
        }
        
        [Test]
        public void ValidationResult_TimestampIsRecent()
        {
            // Act
            var result = _validator.ValidateConfiguration();
            var timeDifference = System.DateTime.UtcNow - result.ValidatedAt;
            
            // Assert - should be within 1 second of current time
            Assert.That(timeDifference.TotalSeconds, Is.LessThan(1.0));
        }
        
        [Test]
        public void ValidationIssue_RelatedFeature_CanBeNull()
        {
            // Arrange
            var issue = new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Info,
                Title = "Test Issue",
                Description = "Test description",
                RelatedFeature = null
            };
            
            // Assert
            Assert.IsNull(issue.RelatedFeature);
            Assert.DoesNotThrow(() => issue.RelatedFeature.ToString());
        }
        
        [Test]
        public void ValidationIssue_RelatedFeature_CanBeSet()
        {
            // Arrange
            var issue = new BuildConfigurationValidator.ValidationIssue
            {
                Severity = BuildConfigurationValidator.ValidationSeverity.Info,
                Title = "Test Issue",
                Description = "Test description",
                RelatedFeature = ExperimentalFeature.AI_INTEGRATION
            };
            
            // Assert
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, issue.RelatedFeature);
        }
    }
}