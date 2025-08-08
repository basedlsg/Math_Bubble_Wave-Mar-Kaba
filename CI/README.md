# CI/CD Performance Gates Integration

This directory contains comprehensive CI/CD integration scripts and configurations for the XR Bubble Library Performance Gates system. The integration implements **Requirement 4.6: CI/CD pipeline integration with build blocking** from the "do-it-right" recovery specification.

## Overview

The CI/CD Performance Gates system provides automated quality assurance that validates code quality and performance requirements before allowing builds to proceed. The system implements four critical performance gates:

1. **Unit Test Gate** - Validates all unit tests pass
2. **Burst Compilation Gate** - Ensures Burst compilation succeeds
3. **Performance Stub Gate** - Validates 60 FPS minimum in editor testing
4. **Performance Profiling Gate** - Validates Quest 3 performance requirements (72 FPS, memory limits)

## Directory Structure

```
CI/
├── Scripts/
│   ├── run-performance-gates.sh      # Unix/Linux/macOS CI script
│   ├── run-performance-gates.bat     # Windows CI script
│   └── analyze-performance.py        # Performance analysis script
├── .github/
│   └── workflows/
│       └── performance-gates.yml     # GitHub Actions workflow
├── Jenkinsfile                       # Jenkins pipeline configuration
└── README.md                         # This file
```

## Quick Start

### Local Testing

```bash
# Unix/Linux/macOS
chmod +x CI/Scripts/run-performance-gates.sh
CI/Scripts/run-performance-gates.sh

# Windows
CI\Scripts\run-performance-gates.bat
```

### GitHub Actions

The GitHub Actions workflow is automatically triggered on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch with custom parameters

### Jenkins

Import the `Jenkinsfile` into your Jenkins instance and configure the pipeline parameters as needed.

## Scripts Reference

### run-performance-gates.sh / run-performance-gates.bat

Cross-platform scripts for running performance gates in CI/CD environments.

#### Usage

```bash
# Run all gates (default)
./run-performance-gates.sh

# Run specific gate
./run-performance-gates.sh gate "Unit Tests"

# Validate configuration only
./run-performance-gates.sh validate

# Generate report only
./run-performance-gates.sh report

# Custom thresholds
./run-performance-gates.sh --min-fps 72 --max-memory 512
```

#### Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `--unity-path PATH` | Path to Unity executable | Auto-detected |
| `--project-path PATH` | Path to Unity project | Current directory |
| `--build-id ID` | Build identifier | Auto-generated |
| `--min-fps FPS` | Minimum FPS threshold | 60.0 |
| `--max-frame-time MS` | Maximum frame time (ms) | 16.67 |
| `--max-memory MB` | Maximum memory usage (MB) | 1024.0 |
| `--fail-on-warnings` | Fail build on warnings | false |
| `--log-file FILE` | Path to log file | performance-gates.log |

#### Environment Variables

| Variable | Description |
|----------|-------------|
| `UNITY_PATH` | Path to Unity executable |
| `PROJECT_PATH` | Path to Unity project |
| `BUILD_ID` | Build identifier |
| `CI_ARTIFACTS_DIR` | Directory for CI artifacts |

#### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success - all gates passed |
| 1 | Failure - one or more gates failed |
| 2 | Fatal error - script execution failed |

### analyze-performance.py

Python script for analyzing performance gate results and generating comprehensive reports.

#### Usage

```bash
# Analyze artifacts directory
python3 CI/Scripts/analyze-performance.py artifacts/

# Custom output directory
python3 CI/Scripts/analyze-performance.py artifacts/ --output-dir reports/

# Verbose output
python3 CI/Scripts/analyze-performance.py artifacts/ --verbose
```

#### Generated Reports

- `performance-analysis-summary.md` - Markdown summary report
- `full-analysis.json` - Complete analysis data in JSON format
- `gate-results.csv` - CSV summary of gate results

## GitHub Actions Integration

### Workflow Features

- **Multi-platform testing** - Linux and Windows runners
- **Matrix builds** - Parallel execution of individual gates
- **Artifact management** - Automatic upload of reports and logs
- **PR comments** - Automatic performance summary comments on pull requests
- **Deployment gates** - Production deployment only after all gates pass

### Workflow Configuration

```yaml
# Custom thresholds via workflow dispatch
on:
  workflow_dispatch:
    inputs:
      min_fps:
        description: 'Minimum FPS threshold'
        default: '60.0'
      max_memory:
        description: 'Maximum memory usage (MB)'
        default: '1024.0'
```

### Required Secrets

| Secret | Description |
|--------|-------------|
| `UNITY_LICENSE` | Unity license file content |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |

### Artifacts

The workflow automatically uploads the following artifacts:

- **Performance Reports** - Detailed performance data and analysis
- **Test Results** - Unit test results and coverage reports
- **Build Logs** - Complete Unity build and gate execution logs
- **Analysis Reports** - Generated analysis summaries and visualizations

## Jenkins Integration

### Pipeline Features

- **Parameterized builds** - Configurable thresholds and Unity versions
- **Parallel execution** - Individual gates run in parallel for faster feedback
- **Quality gates** - Deployment blocking based on critical gate failures
- **Comprehensive reporting** - HTML reports and archived artifacts
- **Notification system** - Build status notifications

### Pipeline Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `MIN_FPS` | String | 60.0 | Minimum FPS threshold |
| `MAX_MEMORY` | String | 1024.0 | Maximum memory usage (MB) |
| `MAX_FRAME_TIME` | String | 16.67 | Maximum frame time (ms) |
| `FAIL_ON_WARNINGS` | Boolean | false | Fail build on warnings |
| `UNITY_VERSION` | Choice | 2022.3.0f1 | Unity version to use |
| `TARGET_PLATFORM` | Choice | StandaloneLinux64 | Target platform |

### Environment Setup

```bash
# Required environment variables
export UNITY_PATH="/opt/Unity/Editor/Unity"
export PROJECT_PATH="${WORKSPACE}/UnityProject"
export BUILD_ID="${BUILD_NUMBER}-${GIT_COMMIT.take(8)}"
```

## Performance Thresholds

### Default Thresholds

| Metric | Default Value | Description |
|--------|---------------|-------------|
| Minimum FPS | 60.0 | Editor testing minimum |
| Maximum Frame Time | 16.67 ms | ~60 FPS frame time |
| Maximum Memory | 1024.0 MB | Quest 3 memory limit |
| Maximum Build Time | 10.0 minutes | Build timeout |
| Maximum Test Time | 5.0 minutes | Test execution timeout |

### Quest 3 Specific Thresholds

| Metric | Value | Description |
|--------|-------|-------------|
| Target FPS | 72.0 | Quest 3 target frame rate |
| Frame Time | 13.89 ms | ~72 FPS frame time |
| Memory Limit | 1024.0 MB | Quest 3 available memory |
| Max GC Alloc | 1.0 MB/frame | Garbage collection limit |
| Max Draw Calls | 500 | Rendering performance limit |
| Max Triangles | 100,000 | Geometry complexity limit |
| Max Texture Memory | 256.0 MB | Texture memory budget |

## Integration Examples

### GitHub Actions Example

```yaml
name: Performance Gates
on: [push, pull_request]

jobs:
  performance-gates:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Run Performance Gates
      run: |
        chmod +x CI/Scripts/run-performance-gates.sh
        CI/Scripts/run-performance-gates.sh --min-fps 72 --max-memory 512
```

### Jenkins Pipeline Example

```groovy
pipeline {
    agent any
    stages {
        stage('Performance Gates') {
            steps {
                sh '''
                    chmod +x CI/Scripts/run-performance-gates.sh
                    CI/Scripts/run-performance-gates.sh all \
                        --min-fps 72 \
                        --max-memory 512 \
                        --build-id ${BUILD_ID}
                '''
            }
        }
    }
}
```

### Azure DevOps Example

```yaml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

steps:
- script: |
    chmod +x CI/Scripts/run-performance-gates.sh
    CI/Scripts/run-performance-gates.sh --build-id $(Build.BuildId)
  displayName: 'Run Performance Gates'
```

### GitLab CI Example

```yaml
stages:
  - test

performance-gates:
  stage: test
  script:
    - chmod +x CI/Scripts/run-performance-gates.sh
    - CI/Scripts/run-performance-gates.sh --build-id $CI_PIPELINE_ID
  artifacts:
    reports:
      junit: PerformanceReports/*.xml
    paths:
      - PerformanceReports/
    expire_in: 30 days
```

## Troubleshooting

### Common Issues

#### Unity Not Found

```bash
# Error: Unity not found at: /path/to/unity
# Solution: Set UNITY_PATH environment variable
export UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity"
```

#### Permission Denied

```bash
# Error: Permission denied
# Solution: Make scripts executable
chmod +x CI/Scripts/run-performance-gates.sh
```

#### Gate Configuration Invalid

```bash
# Error: Gate configuration validation failed
# Solution: Run validation command to see specific issues
CI/Scripts/run-performance-gates.sh validate
```

#### Performance Threshold Violations

```bash
# Error: Performance gates failed - FPS below threshold
# Solution: Adjust thresholds or optimize performance
CI/Scripts/run-performance-gates.sh --min-fps 50 --max-memory 1500
```

### Debug Mode

Enable verbose logging for troubleshooting:

```bash
# Unix/Linux/macOS
export DEBUG=1
CI/Scripts/run-performance-gates.sh

# Windows
set DEBUG=1
CI\Scripts\run-performance-gates.bat
```

### Log Analysis

Performance gate logs are structured for easy analysis:

```bash
# Find failed gates
grep "FAILED" performance-gates.log

# Extract performance metrics
grep "Performance Metrics" performance-gates.log

# Check execution times
grep "Execution Time" performance-gates.log
```

## Best Practices

### CI/CD Pipeline Design

1. **Fail Fast** - Run unit tests first to catch basic issues quickly
2. **Parallel Execution** - Run independent gates in parallel for faster feedback
3. **Artifact Management** - Always archive performance reports for analysis
4. **Threshold Management** - Use environment-specific thresholds
5. **Notification Strategy** - Notify relevant teams of gate failures

### Performance Optimization

1. **Incremental Builds** - Use Unity's incremental build system
2. **Cache Management** - Cache Unity Library folder between builds
3. **Resource Limits** - Set appropriate timeouts and resource limits
4. **Monitoring** - Track gate execution times and optimize slow gates

### Quality Assurance

1. **Regular Threshold Review** - Periodically review and adjust thresholds
2. **Historical Analysis** - Track performance trends over time
3. **Gate Maintenance** - Keep gates updated with project requirements
4. **Documentation** - Maintain clear documentation for gate failures

## Support and Maintenance

### Updating Thresholds

Performance thresholds should be reviewed regularly and updated based on:

- Hardware capability changes (e.g., Quest 3 Pro)
- Performance optimization improvements
- User experience requirements
- Business requirements changes

### Adding Custom Gates

To add custom performance gates:

1. Implement `IPerformanceGate` interface
2. Register gate in `PerformanceGateRunner`
3. Update CI scripts to handle new gate
4. Add appropriate thresholds and validation

### Monitoring and Alerting

Consider implementing:

- Performance trend monitoring
- Automated threshold adjustment
- Integration with monitoring systems (Grafana, DataDog, etc.)
- Slack/Teams notifications for gate failures

## Contributing

When contributing to the CI/CD system:

1. Test changes locally before committing
2. Update documentation for any new features
3. Ensure backward compatibility with existing pipelines
4. Add appropriate error handling and logging
5. Update threshold documentation if needed

## License

This CI/CD integration is part of the XR Bubble Library project and follows the same licensing terms.

---

**Status**: PRODUCTION READY  
**Maintenance**: ACTIVE  
**Support**: FULL SUPPORT FOR ALL MAJOR CI/CD PLATFORMS  

For questions or issues, please refer to the project documentation or create an issue in the project repository.