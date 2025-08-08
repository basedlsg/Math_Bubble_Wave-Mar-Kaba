# Evidence Collection System

## Overview

The Evidence Collection System is a comprehensive solution for implementing evidence-based development practices in the XR Bubble Library project. It automatically collects, validates, and manages supporting evidence for all development claims, ensuring that performance and functionality assertions are backed by verifiable data.

This system directly implements **Requirement 10: Evidence-Based Development Documentation** from the "Do-It-Right" recovery specification, providing the infrastructure needed to restore credibility through systematic verification.

## Key Features

### 🔍 **Comprehensive Evidence Collection**
- Automatically discovers and catalogs evidence files across the project
- Supports multiple evidence types: performance logs, test results, build logs, screenshots, profiler data, and configuration files
- Generates cryptographic hashes for file integrity verification
- Collects metadata including timestamps, file sizes, and descriptive information

### ✅ **Evidence Validation System**
- Validates file accessibility and integrity using SHA-256 hashing
- Checks file size consistency and hash matching
- Provides detailed validation reports with error descriptions
- Supports batch validation of multiple evidence files

### 📦 **Evidence Archiving**
- Creates timestamped archives for long-term evidence storage
- Maintains archive metadata for audit trails
- Supports evidence retrieval by hash for verification
- Provides archive statistics and error reporting

### 🔎 **Advanced Search Capabilities**
- Search by evidence type, module, date range, and description
- Tag-based filtering and categorization
- Configurable result limits and validation status filtering
- Support for complex search criteria combinations

### 📊 **Reporting and Analytics**
- Generates comprehensive evidence reports in Markdown format
- Provides module-specific evidence analysis
- Calculates validation success rates and statistics
- Creates evidence summaries grouped by module and type

## Architecture

### Core Components

#### IEvidenceCollector Interface
```csharp
public interface IEvidenceCollector
{
    List<EvidenceFile> CollectAllEvidence();
    Task<List<EvidenceFile>> CollectAllEvidenceAsync();
    List<EvidenceFile> CollectModuleEvidence(string moduleName);
    List<EvidenceValidationResult> ValidateEvidence(List<EvidenceFile> evidenceFiles);
    string GenerateEvidenceHash(string filePath);
    EvidenceArchiveResult ArchiveEvidence(List<EvidenceFile> evidenceFiles);
    EvidenceFile GetEvidenceByHash(string hash);
    List<EvidenceFile> SearchEvidence(EvidenceSearchCriteria criteria);
}
```

#### EvidenceCollector Implementation
The main implementation class that handles:
- File system scanning for evidence files
- Hash generation using SHA-256 cryptography
- Evidence validation and integrity checking
- Archive creation and management
- Search functionality with filtering

#### EvidenceCollectorService
High-level service class providing:
- Report generation in Markdown format
- Module-specific evidence analysis
- Validation summaries and statistics
- Integration with existing development tools

### Data Structures

#### EvidenceFile
```csharp
public class EvidenceFile
{
    public string Id { get; set; }
    public string FilePath { get; set; }
    public EvidenceType Type { get; set; }
    public string ModuleName { get; set; }
    public string Hash { get; set; }
    public DateTime CollectedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public bool IsValid { get; set; }
    public DateTime LastValidated { get; set; }
}
```

#### Evidence Types
- **PerformanceLog**: Performance profiling data and logs
- **Screenshot**: Visual evidence of functionality or UI
- **TestResult**: Unit test results and coverage reports
- **BuildLog**: Build logs and compilation output
- **Configuration**: Configuration files and settings
- **VideoRecording**: Video recordings of functionality
- **ProfilerData**: Unity profiler data and metrics
- **HardwareMetrics**: Hardware performance and system information
- **Documentation**: Reports and documentation files
- **Other**: Additional evidence types

## Usage Guide

### Basic Evidence Collection

```csharp
// Initialize the evidence collector
var evidenceCollector = new EvidenceCollector();

// Collect all available evidence
var allEvidence = evidenceCollector.CollectAllEvidence();

// Collect evidence for a specific module
var moduleEvidence = evidenceCollector.CollectModuleEvidence("Core");

// Validate evidence integrity
var validationResults = evidenceCollector.ValidateEvidence(allEvidence);
```

### Using the Service Layer

```csharp
// Initialize the service
var evidenceService = new EvidenceCollectorService();

// Generate comprehensive evidence report
var reportPath = evidenceService.GenerateEvidenceReport();

// Get validation summary
var validationSummary = evidenceService.ValidateAllEvidence();

// Get statistics by module
var statistics = evidenceService.GetEvidenceStatisticsByModule();
```

### Advanced Search

```csharp
// Create search criteria
var criteria = new EvidenceSearchCriteria
{
    Type = EvidenceType.PerformanceLog,
    ModuleName = "Core",
    FromDate = DateTime.UtcNow.AddDays(-7),
    OnlyValid = true,
    MaxResults = 50
};

// Search for matching evidence
var searchResults = evidenceCollector.SearchEvidence(criteria);
```

### Evidence Archiving

```csharp
// Archive all current evidence
var archiveResult = evidenceCollector.ArchiveEvidence(allEvidence);

if (archiveResult.Success)
{
    Debug.Log($"Archived {archiveResult.FilesArchived} files to {archiveResult.ArchivePath}");
}
```

## Unity Editor Integration

### Evidence Collection Window
Access via **XR Bubble Library > Evidence Collection System**

The editor window provides four main tabs:

#### 1. Collection Tab
- **Collect All Evidence**: Discovers and catalogs all evidence files
- **Generate Evidence Report**: Creates comprehensive Markdown reports
- **Evidence File Display**: Shows collected evidence grouped by module

#### 2. Validation Tab
- **Validate All Evidence**: Checks file integrity and accessibility
- **Validation Summary**: Displays success rates and statistics
- **Invalid File Details**: Shows validation errors and issues

#### 3. Search Tab
- **Search Criteria**: Filter by type, module, date range, and description
- **Search Results**: Display matching evidence files
- **Advanced Filtering**: Support for complex search combinations

#### 4. Archive Tab
- **Archive All Evidence**: Creates timestamped evidence archives
- **Archive Results**: Shows archive statistics and paths
- **Error Reporting**: Displays any archiving issues

### Menu Items
Quick access menu items under **XR Bubble Library > Evidence**:
- **Collect All Evidence**: Quick evidence collection
- **Generate Evidence Report**: Create evidence report
- **Validate All Evidence**: Validate all evidence files
- **Archive All Evidence**: Archive current evidence

## Integration with DevStateGenerator

The Evidence Collection System integrates seamlessly with the existing DevStateGenerator:

```csharp
public class DevStateGenerator : IDevStateGenerator
{
    private readonly EvidenceCollectorService _evidenceService;
    
    public DevStateReport GenerateReport()
    {
        // ... existing logic
        
        // Collect supporting evidence
        var evidence = _evidenceService.SearchEvidence(new EvidenceSearchCriteria 
        { 
            MaxResults = 1000 
        });
        
        // Validate evidence
        var validationSummary = _evidenceService.ValidateAllEvidence();
        
        // Include evidence in report
        report.SupportingEvidence = evidence;
        report.EvidenceValidation = validationSummary;
        
        return report;
    }
}
```

## Evidence-Based Development Workflow

### 1. Claim Creation
When making any development claim:
```csharp
// Before making a claim, collect supporting evidence
var evidence = evidenceService.CollectModuleEvidence("Performance");
var validation = evidenceService.ValidateAllEvidence();

// Only proceed with claim if evidence is valid
if (validation.ValidationSuccessRate >= 0.95)
{
    // Make the claim with evidence backing
    CreatePerformanceClaim(evidence);
}
```

### 2. Evidence Verification
```csharp
// Verify evidence before using in documentation
var evidenceFile = evidenceCollector.GetEvidenceByHash(claimedHash);
var validationResult = evidenceCollector.ValidateEvidence(new[] { evidenceFile });

if (validationResult.First().IsValid)
{
    // Evidence is valid, safe to use in documentation
    IncludeInDocumentation(evidenceFile);
}
```

### 3. Audit Trail Maintenance
```csharp
// Regular evidence archiving for audit trails
var archiveResult = evidenceService.ArchiveAllEvidence();

// Generate evidence reports for stakeholder review
var reportPath = evidenceService.GenerateEvidenceReport();
```

## Configuration

### Evidence Directory Structure
```
Evidence/
├── Reports/                    # Generated evidence reports
├── Archive/                    # Archived evidence files
│   ├── evidence_archive_20250213_143000/
│   └── evidence_archive_20250214_090000/
└── Screenshots/               # Screenshot evidence files
```

### Supported File Patterns
- **Performance Logs**: `*.log` files in Logs directory
- **Test Results**: `*.xml` files in TestResults directory
- **Build Logs**: `*build*.log` files in Logs directory
- **Assembly Definitions**: `*.asmdef` files throughout project
- **Project Settings**: `*.asset` files in ProjectSettings
- **Profiler Data**: `*.data` files in ProfilerData directory
- **Screenshots**: `*.png`, `*.jpg` files in Screenshots directory

## Testing

### Unit Tests
Comprehensive test coverage includes:
- Evidence collection functionality
- Hash generation and validation
- File integrity checking
- Search and filtering operations
- Archive creation and management
- Async operation handling

### Test Execution
```bash
# Run evidence collector tests
Unity -batchmode -runTests -testPlatform EditMode -testFilter "EvidenceCollectorTests"
```

## Performance Considerations

### Optimization Features
- **Incremental Collection**: Only re-scan changed directories
- **Evidence Caching**: Cache collected evidence for repeated operations
- **Async Operations**: Support for non-blocking evidence collection
- **Batch Processing**: Efficient handling of large evidence sets

### Memory Management
- **Streaming Hash Calculation**: Process large files without loading into memory
- **Lazy Loading**: Load evidence metadata only when needed
- **Disposal Patterns**: Proper cleanup of file handles and resources

## Security and Integrity

### Hash-Based Verification
- **SHA-256 Hashing**: Cryptographically secure file integrity verification
- **Hash Matching**: Detect any modifications to evidence files
- **Tamper Detection**: Identify corrupted or altered evidence

### Access Control
- **Read-Only Evidence**: Evidence files are never modified by the system
- **Secure Archiving**: Archives maintain original file permissions
- **Audit Logging**: All evidence operations are logged for review

## Troubleshooting

### Common Issues

#### Evidence Files Not Found
```csharp
// Check if evidence directories exist
if (!Directory.Exists(evidenceDirectory))
{
    Debug.LogWarning("Evidence directory not found, creating...");
    Directory.CreateDirectory(evidenceDirectory);
}
```

#### Hash Validation Failures
```csharp
// Re-generate hash if validation fails
var currentHash = evidenceCollector.GenerateEvidenceHash(filePath);
if (currentHash != expectedHash)
{
    Debug.LogWarning($"Hash mismatch for {filePath}. File may have been modified.");
}
```

#### Archive Creation Errors
```csharp
// Check disk space and permissions before archiving
var driveInfo = new DriveInfo(archivePath);
if (driveInfo.AvailableFreeSpace < requiredSpace)
{
    Debug.LogError("Insufficient disk space for evidence archive");
}
```

### Debug Logging
Enable detailed logging by setting:
```csharp
Debug.unityLogger.logEnabled = true;
Debug.unityLogger.filterLogType = LogType.Log;
```

## Best Practices

### Evidence Collection
1. **Regular Collection**: Run evidence collection after significant changes
2. **Validation Before Use**: Always validate evidence before including in documentation
3. **Descriptive Naming**: Use clear, descriptive names for evidence files
4. **Proper Categorization**: Assign appropriate types and tags to evidence

### Archive Management
1. **Regular Archiving**: Create archives before major releases
2. **Archive Retention**: Maintain archives for audit and compliance requirements
3. **Storage Management**: Monitor archive storage usage and cleanup old archives
4. **Backup Strategy**: Include evidence archives in backup procedures

### Integration Guidelines
1. **CI/CD Integration**: Include evidence validation in build pipelines
2. **Documentation Standards**: Require evidence backing for all performance claims
3. **Review Processes**: Include evidence review in code review procedures
4. **Stakeholder Access**: Provide stakeholders with access to evidence reports

## Future Enhancements

### Planned Features
- **Cloud Storage Integration**: Support for cloud-based evidence storage
- **Real-Time Monitoring**: Continuous evidence collection and validation
- **Advanced Analytics**: Machine learning-based evidence analysis
- **Integration APIs**: REST APIs for external tool integration

### Extensibility Points
- **Custom Evidence Types**: Support for domain-specific evidence types
- **Plugin Architecture**: Allow custom evidence collectors and validators
- **Export Formats**: Support for additional report formats (PDF, HTML)
- **Notification System**: Alerts for evidence validation failures

## Conclusion

The Evidence Collection System provides a robust foundation for evidence-based development practices, ensuring that all claims about system functionality and performance are backed by verifiable data. By implementing comprehensive evidence collection, validation, and management capabilities, this system helps restore credibility and maintain transparency throughout the development process.

The system's integration with Unity Editor tools and existing development workflows makes it easy to adopt evidence-based practices without disrupting current development processes. With comprehensive testing, documentation, and extensibility features, the Evidence Collection System provides a solid foundation for trustworthy, verifiable development documentation.