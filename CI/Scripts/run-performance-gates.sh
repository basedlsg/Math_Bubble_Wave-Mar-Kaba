#!/bin/bash

# Performance Gates CI/CD Script for Unix-based systems (Linux/macOS)
# This script runs Unity performance gates in CI/CD environments
# Implements Requirement 4.6: CI/CD pipeline integration with build blocking

set -e  # Exit on any error

# Configuration
UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity}"
PROJECT_PATH="${PROJECT_PATH:-$(pwd)}"
LOG_FILE="${LOG_FILE:-performance-gates.log}"
REPORT_DIR="${REPORT_DIR:-./PerformanceReports}"
BUILD_ID="${BUILD_ID:-local-$(date +%Y%m%d-%H%M%S)}"

# Performance thresholds (can be overridden by environment variables)
MIN_FPS="${MIN_FPS:-60.0}"
MAX_FRAME_TIME="${MAX_FRAME_TIME:-16.67}"
MAX_MEMORY="${MAX_MEMORY:-1024.0}"
MAX_BUILD_TIME="${MAX_BUILD_TIME:-10.0}"
MAX_TEST_TIME="${MAX_TEST_TIME:-5.0}"
FAIL_ON_WARNINGS="${FAIL_ON_WARNINGS:-false}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
}

# Function to check if Unity is available
check_unity() {
    if [ ! -f "$UNITY_PATH" ]; then
        log_error "Unity not found at: $UNITY_PATH"
        log_error "Please set UNITY_PATH environment variable or install Unity"
        exit 1
    fi
    
    log_info "Unity found at: $UNITY_PATH"
}

# Function to validate project structure
validate_project() {
    if [ ! -d "$PROJECT_PATH/Assets" ]; then
        log_error "Unity project not found at: $PROJECT_PATH"
        log_error "Please set PROJECT_PATH environment variable"
        exit 1
    fi
    
    if [ ! -f "$PROJECT_PATH/Assets/XRBubbleLibrary/Core/Editor/PerformanceGateCommandLine.cs" ]; then
        log_error "Performance gate command line interface not found"
        log_error "Please ensure XRBubbleLibrary is properly installed"
        exit 1
    fi
    
    log_info "Project structure validated"
}

# Function to run Unity command with proper error handling
run_unity_command() {
    local method="$1"
    local additional_args="$2"
    
    log_info "Running Unity command: $method"
    
    # Create log directory
    mkdir -p "$(dirname "$LOG_FILE")"
    
    # Run Unity in batch mode
    "$UNITY_PATH" \
        -batchmode \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -logFile "$LOG_FILE" \
        -executeMethod "$method" \
        -buildId "$BUILD_ID" \
        -minFPS "$MIN_FPS" \
        -maxFrameTime "$MAX_FRAME_TIME" \
        -maxMemory "$MAX_MEMORY" \
        -maxBuildTime "$MAX_BUILD_TIME" \
        -maxTestTime "$MAX_TEST_TIME" \
        -failOnWarnings "$FAIL_ON_WARNINGS" \
        $additional_args
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        log_success "Unity command completed successfully"
        return 0
    elif [ $exit_code -eq 1 ]; then
        log_error "Unity command failed - performance gates blocked the build"
        return 1
    elif [ $exit_code -eq 2 ]; then
        log_error "Unity command failed - fatal error during execution"
        return 2
    else
        log_error "Unity command failed with unexpected exit code: $exit_code"
        return $exit_code
    fi
}

# Function to run all performance gates
run_all_gates() {
    log_info "=== Running All Performance Gates ==="
    log_info "Build ID: $BUILD_ID"
    log_info "Thresholds: FPS>=$MIN_FPS, FrameTime<=$MAX_FRAME_TIME ms, Memory<=$MAX_MEMORY MB"
    
    if run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.RunAllGatesFromCommandLine"; then
        log_success "All performance gates PASSED"
        return 0
    else
        log_error "Performance gates FAILED - build blocked"
        return 1
    fi
}

# Function to run a specific gate
run_specific_gate() {
    local gate_name="$1"
    
    if [ -z "$gate_name" ]; then
        log_error "Gate name not specified"
        echo "Usage: $0 --gate \"Gate Name\""
        exit 1
    fi
    
    log_info "=== Running Specific Gate: $gate_name ==="
    
    if run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.RunSpecificGateFromCommandLine" "-gateName \"$gate_name\""; then
        log_success "Gate '$gate_name' PASSED"
        return 0
    else
        log_error "Gate '$gate_name' FAILED"
        return 1
    fi
}

# Function to validate configuration
validate_configuration() {
    log_info "=== Validating Performance Gate Configuration ==="
    
    if run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.ValidateGateConfigurationFromCommandLine"; then
        log_success "Configuration validation PASSED"
        return 0
    else
        log_error "Configuration validation FAILED"
        return 1
    fi
}

# Function to generate report
generate_report() {
    log_info "=== Generating Performance Report ==="
    
    if run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.GenerateReportFromCommandLine"; then
        log_success "Performance report generated"
        
        # Check if report path file exists
        local report_path_file="$PROJECT_PATH/../performance_report_path.txt"
        if [ -f "$report_path_file" ]; then
            local report_path=$(cat "$report_path_file")
            log_info "Report available at: $report_path"
            
            # Copy report to CI artifacts directory if specified
            if [ -n "$CI_ARTIFACTS_DIR" ]; then
                mkdir -p "$CI_ARTIFACTS_DIR"
                cp "$report_path" "$CI_ARTIFACTS_DIR/"
                log_info "Report copied to CI artifacts: $CI_ARTIFACTS_DIR"
            fi
        fi
        
        return 0
    else
        log_error "Report generation FAILED"
        return 1
    fi
}

# Function to display usage
show_usage() {
    echo "Usage: $0 [OPTIONS] [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  all                    Run all performance gates (default)"
    echo "  gate <name>           Run specific performance gate"
    echo "  validate              Validate gate configuration only"
    echo "  report                Generate performance report only"
    echo ""
    echo "Options:"
    echo "  --unity-path PATH     Path to Unity executable"
    echo "  --project-path PATH   Path to Unity project"
    echo "  --log-file FILE       Path to log file"
    echo "  --build-id ID         Build identifier"
    echo "  --min-fps FPS         Minimum FPS threshold (default: 60.0)"
    echo "  --max-frame-time MS   Maximum frame time in ms (default: 16.67)"
    echo "  --max-memory MB       Maximum memory usage in MB (default: 1024.0)"
    echo "  --fail-on-warnings    Fail build on warnings (default: false)"
    echo "  --help                Show this help message"
    echo ""
    echo "Environment Variables:"
    echo "  UNITY_PATH            Path to Unity executable"
    echo "  PROJECT_PATH          Path to Unity project"
    echo "  BUILD_ID              Build identifier"
    echo "  CI_ARTIFACTS_DIR      Directory to copy reports for CI artifacts"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Run all gates"
    echo "  $0 gate \"Unit Tests\"                # Run specific gate"
    echo "  $0 validate                          # Validate configuration"
    echo "  $0 --min-fps 72 --max-memory 512    # Custom thresholds"
}

# Main execution
main() {
    local command="all"
    local gate_name=""
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --unity-path)
                UNITY_PATH="$2"
                shift 2
                ;;
            --project-path)
                PROJECT_PATH="$2"
                shift 2
                ;;
            --log-file)
                LOG_FILE="$2"
                shift 2
                ;;
            --build-id)
                BUILD_ID="$2"
                shift 2
                ;;
            --min-fps)
                MIN_FPS="$2"
                shift 2
                ;;
            --max-frame-time)
                MAX_FRAME_TIME="$2"
                shift 2
                ;;
            --max-memory)
                MAX_MEMORY="$2"
                shift 2
                ;;
            --fail-on-warnings)
                FAIL_ON_WARNINGS="true"
                shift
                ;;
            --help)
                show_usage
                exit 0
                ;;
            all)
                command="all"
                shift
                ;;
            gate)
                command="gate"
                gate_name="$2"
                shift 2
                ;;
            validate)
                command="validate"
                shift
                ;;
            report)
                command="report"
                shift
                ;;
            *)
                log_error "Unknown argument: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Initialize
    log_info "=== Performance Gates CI/CD Script ==="
    log_info "Command: $command"
    log_info "Build ID: $BUILD_ID"
    log_info "Project Path: $PROJECT_PATH"
    
    # Validate environment
    check_unity
    validate_project
    
    # Execute command
    case $command in
        all)
            if run_all_gates; then
                log_success "=== ALL PERFORMANCE GATES PASSED ==="
                exit 0
            else
                log_error "=== PERFORMANCE GATES FAILED - BUILD BLOCKED ==="
                exit 1
            fi
            ;;
        gate)
            if run_specific_gate "$gate_name"; then
                log_success "=== GATE PASSED ==="
                exit 0
            else
                log_error "=== GATE FAILED ==="
                exit 1
            fi
            ;;
        validate)
            if validate_configuration; then
                log_success "=== CONFIGURATION VALID ==="
                exit 0
            else
                log_error "=== CONFIGURATION INVALID ==="
                exit 1
            fi
            ;;
        report)
            if generate_report; then
                log_success "=== REPORT GENERATED ==="
                exit 0
            else
                log_error "=== REPORT GENERATION FAILED ==="
                exit 1
            fi
            ;;
        *)
            log_error "Unknown command: $command"
            show_usage
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"