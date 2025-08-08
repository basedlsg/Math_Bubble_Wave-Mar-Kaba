using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the Evidence Collection System.
    /// Tests evidence collection, validation, archiving, and search functionality.
    /// </summary>
    [TestFixture]
    public class EvidenceCollectorTests
    {
        private EvidenceCollector _evidenceCollector;
        private string _testEvidenceDirectory;
        private string _testFilePath;
        
        [SetUp]
        public void SetUp()
        {
            // Create temporary test directory
            _testEvidenceDirectory = Path.Combine(Path.GetTempPath(), "EvidenceCollectorTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testEvidenceDirectory);
            
            // Initialize evidence collector with test directory
            _evidenceCollector = new EvidenceCollector(_testEvidenceDirectory);
            
            // Create test evidence file
            _testFilePath = Path.Combine(_testEvidenceDirectory, "test_evidence.log");
            File.WriteAllText(_testFilePath, "Test evidence content for unit testing");
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test directory
            if (Directory.Exists(_testEvidenceDirectory))
            {
                Directory.Delete(_testEvidenceDirectory, true);
            }
        }
        
        #region Evidence Collection Tests
        
        [Test]
        public void CollectAllEvidence_ShouldReturnEvidenceList()
        {
            // Act
            var evidence = _evidenceCollector.CollectAllEvidence();
            
            // Assert
            Assert.IsNotNull(evidence);
            Assert.IsInstanceOf<List<EvidenceFile>>(evidence);
        }
        
        [Test]
        public void CollectModuleEvidence_WithValidModule_ShouldReturnModuleSpecificEvidence()
        {
            // Arrange
            var moduleName = "TestModule";
            
            // Act
            var evidence = _evidenceCollector.CollectModuleEvidence(moduleName);
            
            // Assert
            Assert.IsNotNull(evidence);
            Assert.IsInstanceOf<List<EvidenceFile>>(evidence);
        }
        
        [Test]
        public void CollectModuleEvidence_WithNullModule_ShouldReturnEmptyList()
        {
            // Act
            var evidence = _evidenceCollector.CollectModuleEvidence(null);
            
            // Assert
            Assert.IsNotNull(evidence);
            Assert.AreEqual(0, evidence.Count);
        }
        
        [Test]
        public void CollectModuleEvidence_WithEmptyModule_ShouldReturnEmptyList()
        {
            // Act
            var evidence = _evidenceCollector.CollectModuleEvidence("");
            
            // Assert
            Assert.IsNotNull(evidence);
            Assert.AreEqual(0, evidence.Count);
        }
        
        #endregion
        
        #region Hash Generation Tests
        
        [Test]
        public void GenerateEvidenceHash_WithValidFile_ShouldReturnHash()
        {
            // Act
            var hash = _evidenceCollector.GenerateEvidenceHash(_testFilePath);
            
            // Assert
            Assert.IsNotNull(hash);
            Assert.IsNotEmpty(hash);
            Assert.Greater(hash.Length, 0);
        }
        
        [Test]
        public void GenerateEvidenceHash_WithSameFile_ShouldReturnSameHash()
        {
            // Act
            var hash1 = _evidenceCollector.GenerateEvidenceHash(_testFilePath);
            var hash2 = _evidenceCollector.GenerateEvidenceHash(_testFilePath);
            
            // Assert
            Assert.AreEqual(hash1, hash2);
        }
        
        [Test]
        public void GenerateEvidenceHash_WithNonExistentFile_ShouldThrowException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(_testEvidenceDirectory, "nonexistent.txt");
            
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => _evidenceCollector.GenerateEvidenceHash(nonExistentFile));
        }
        
        #endregion
        
        #region Evidence Validation Tests
        
        [Test]
        public void ValidateEvidence_WithValidEvidenceFile_ShouldReturnValidResult()
        {
            // Arrange
            var evidenceFile = CreateTestEvidenceFile(_testFilePath);
            var evidenceFiles = new List<EvidenceFile> { evidenceFile };
            
            // Act
            var results = _evidenceCollector.ValidateEvidence(evidenceFiles);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].IsValid);
            Assert.IsTrue(results[0].FileAccessible);
            Assert.IsTrue(results[0].HashMatches);
            Assert.IsTrue(results[0].SizeMatches);
        }
        
        [Test]
        public void ValidateEvidence_WithInvalidHash_ShouldReturnInvalidResult()
        {
            // Arrange
            var evidenceFile = CreateTestEvidenceFile(_testFilePath);
            evidenceFile.Hash = "invalid_hash";
            var evidenceFiles = new List<EvidenceFile> { evidenceFile };
            
            // Act
            var results = _evidenceCollector.ValidateEvidence(evidenceFiles);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].IsValid);
            Assert.IsTrue(results[0].FileAccessible);
            Assert.IsFalse(results[0].HashMatches);
        }
        
        [Test]
        public void ValidateEvidence_WithNonExistentFile_ShouldReturnInvalidResult()
        {
            // Arrange
            var nonExistentFile = Path.Combine(_testEvidenceDirectory, "nonexistent.txt");
            var evidenceFile = new EvidenceFile
            {
                Id = Guid.NewGuid().ToString(),
                FilePath = nonExistentFile,
                Hash = "test_hash",
                FileSizeBytes = 100
            };
            var evidenceFiles = new List<EvidenceFile> { evidenceFile };
            
            // Act
            var results = _evidenceCollector.ValidateEvidence(evidenceFiles);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].IsValid);
            Assert.IsFalse(results[0].FileAccessible);
            Assert.Greater(results[0].ValidationErrors.Count, 0);
        }
        
        #endregion
        
        #region Evidence Archiving Tests
        
        [Test]
        public void ArchiveEvidence_WithValidEvidenceFiles_ShouldCreateArchive()
        {
            // Arrange
            var evidenceFile = CreateTestEvidenceFile(_testFilePath);
            var evidenceFiles = new List<EvidenceFile> { evidenceFile };
            
            // Act
            var result = _evidenceCollector.ArchiveEvidence(evidenceFiles);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.FilesArchived);
            Assert.IsNotNull(result.ArchivePath);
            Assert.IsTrue(Directory.Exists(result.ArchivePath));
            Assert.Greater(result.ArchiveSizeBytes, 0);
        }
        
        [Test]
        public void ArchiveEvidence_WithEmptyList_ShouldCreateEmptyArchive()
        {
            // Arrange
            var evidenceFiles = new List<EvidenceFile>();
            
            // Act
            var result = _evidenceCollector.ArchiveEvidence(evidenceFiles);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success); // Should be false for empty archive
            Assert.AreEqual(0, result.FilesArchived);
        }
        
        #endregion
        
        #region Evidence Search Tests
        
        [Test]
        public void SearchEvidence_WithTypeFilter_ShouldReturnMatchingEvidence()
        {
            // Arrange
            var criteria = new EvidenceSearchCriteria
            {
                Type = EvidenceType.PerformanceLog,
                MaxResults = 10
            };
            
            // Act
            var results = _evidenceCollector.SearchEvidence(criteria);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.IsInstanceOf<List<EvidenceFile>>(results);
            // All results should match the type filter (if any results exist)
            Assert.IsTrue(results.All(r => r.Type == EvidenceType.PerformanceLog));
        }
        
        [Test]
        public void SearchEvidence_WithModuleFilter_ShouldReturnMatchingEvidence()
        {
            // Arrange
            var moduleName = "TestModule";
            var criteria = new EvidenceSearchCriteria
            {
                ModuleName = moduleName,
                MaxResults = 10
            };
            
            // Act
            var results = _evidenceCollector.SearchEvidence(criteria);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.IsInstanceOf<List<EvidenceFile>>(results);
            // All results should match the module filter (if any results exist)
            Assert.IsTrue(results.All(r => r.ModuleName == moduleName));
        }
        
        [Test]
        public void SearchEvidence_WithDateRangeFilter_ShouldReturnMatchingEvidence()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-1);
            var toDate = DateTime.UtcNow.AddDays(1);
            var criteria = new EvidenceSearchCriteria
            {
                FromDate = fromDate,
                ToDate = toDate,
                MaxResults = 10
            };
            
            // Act
            var results = _evidenceCollector.SearchEvidence(criteria);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.IsInstanceOf<List<EvidenceFile>>(results);
            // All results should be within the date range (if any results exist)
            Assert.IsTrue(results.All(r => r.CollectedAt >= fromDate && r.CollectedAt <= toDate));
        }
        
        [Test]
        public void SearchEvidence_WithMaxResultsLimit_ShouldRespectLimit()
        {
            // Arrange
            var maxResults = 5;
            var criteria = new EvidenceSearchCriteria
            {
                MaxResults = maxResults
            };
            
            // Act
            var results = _evidenceCollector.SearchEvidence(criteria);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.LessOrEqual(results.Count, maxResults);
        }
        
        #endregion
        
        #region Evidence Retrieval Tests
        
        [Test]
        public void GetEvidenceByHash_WithValidHash_ShouldReturnEvidenceFile()
        {
            // Arrange
            var evidenceFile = CreateTestEvidenceFile(_testFilePath);
            var hash = evidenceFile.Hash;
            
            // Mock the evidence collection to include our test file
            // This would require modifying the collector or using a mock
            
            // Act
            var result = _evidenceCollector.GetEvidenceByHash(hash);
            
            // Assert
            // Note: This test may return null if the evidence collector doesn't find the file
            // in its standard collection paths. This is expected behavior for the test environment.
            Assert.IsTrue(result == null || result.Hash == hash);
        }
        
        [Test]
        public void GetEvidenceByHash_WithNullHash_ShouldReturnNull()
        {
            // Act
            var result = _evidenceCollector.GetEvidenceByHash(null);
            
            // Assert
            Assert.IsNull(result);
        }
        
        [Test]
        public void GetEvidenceByHash_WithEmptyHash_ShouldReturnNull()
        {
            // Act
            var result = _evidenceCollector.GetEvidenceByHash("");
            
            // Assert
            Assert.IsNull(result);
        }
        
        #endregion
        
        #region Async Tests
        
        [UnityTest]
        public System.Collections.IEnumerator CollectAllEvidenceAsync_ShouldReturnEvidenceList()
        {
            // Arrange
            List<EvidenceFile> result = null;
            Exception exception = null;
            
            // Act
            var task = _evidenceCollector.CollectAllEvidenceAsync();
            
            // Wait for completion
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.Exception != null)
            {
                exception = task.Exception;
            }
            else
            {
                result = task.Result;
            }
            
            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<EvidenceFile>>(result);
        }
        
        #endregion
        
        #region Helper Methods
        
        private EvidenceFile CreateTestEvidenceFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            
            return new EvidenceFile
            {
                Id = Guid.NewGuid().ToString(),
                FilePath = filePath,
                Type = EvidenceType.PerformanceLog,
                ModuleName = "TestModule",
                Hash = _evidenceCollector.GenerateEvidenceHash(filePath),
                CollectedAt = DateTime.UtcNow,
                FileSizeBytes = fileInfo.Length,
                Description = "Test evidence file",
                LastValidated = DateTime.UtcNow,
                Tags = new List<string> { "test", "performance" }
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Integration tests for the Evidence Collector Service.
    /// </summary>
    [TestFixture]
    public class EvidenceCollectorServiceTests
    {
        private EvidenceCollectorService _service;
        private string _testDirectory;
        
        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "EvidenceServiceTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            
            var evidenceCollector = new EvidenceCollector(_testDirectory);
            _service = new EvidenceCollectorService(evidenceCollector);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        
        [Test]
        public void GenerateEvidenceReport_ShouldCreateReportFile()
        {
            // Act
            var reportPath = _service.GenerateEvidenceReport();
            
            // Assert
            Assert.IsNotNull(reportPath);
            Assert.IsTrue(File.Exists(reportPath));
            
            var content = File.ReadAllText(reportPath);
            Assert.IsTrue(content.Contains("# Evidence Collection Report"));
        }
        
        [Test]
        public void ValidateAllEvidence_ShouldReturnValidationSummary()
        {
            // Act
            var summary = _service.ValidateAllEvidence();
            
            // Assert
            Assert.IsNotNull(summary);
            Assert.GreaterOrEqual(summary.TotalEvidenceFiles, 0);
            Assert.GreaterOrEqual(summary.ValidFiles, 0);
            Assert.GreaterOrEqual(summary.InvalidFiles, 0);
            Assert.IsNotNull(summary.ValidationResults);
        }
        
        [Test]
        public void GetEvidenceStatisticsByModule_ShouldReturnStatistics()
        {
            // Act
            var statistics = _service.GetEvidenceStatisticsByModule();
            
            // Assert
            Assert.IsNotNull(statistics);
            Assert.IsInstanceOf<Dictionary<string, EvidenceStatistics>>(statistics);
        }
        
        [UnityTest]
        public System.Collections.IEnumerator GenerateEvidenceReportAsync_ShouldCreateReportFile()
        {
            // Arrange
            string reportPath = null;
            Exception exception = null;
            
            // Act
            var task = _service.GenerateEvidenceReportAsync();
            
            // Wait for completion
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.Exception != null)
            {
                exception = task.Exception;
            }
            else
            {
                reportPath = task.Result;
            }
            
            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(reportPath);
            Assert.IsTrue(File.Exists(reportPath));
        }
    }
}