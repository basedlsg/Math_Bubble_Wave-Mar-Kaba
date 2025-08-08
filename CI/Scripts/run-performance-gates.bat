@echo off
REM Performance Gates CI/CD Script for Windows
REM This script runs Unity performance gates in CI/CD environments
REM Implements Requirement 4.6: CI/CD pipeline integration with build blocking

setlocal enabledelayedexpansion

REM Configuration
if "%UNITY_PATH%"=="" set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.0f1\Editor\Unity.exe
if "%PROJECT_PATH%"=="" set PROJECT_PATH=%CD%
if "%LOG_FILE%"=="" set LOG_FILE=performance-gates.log
if "%BUILD_ID%"=="" (
    for /f "tokens=1-3 delims=/ " %%a in ('date /t') do set BUILD_DATE=%%c%%a%%b
    for /f "tokens=1-2 delims=: " %%a in ('time /t') do set BUILD_TIME=%%a%%b
    set BUILD_ID=local-!BUILD_DATE!-!BUILD_TIME!
)

REM Performance thresholds (can be overridden by environment variables)
if "%MIN_FPS%"=="" set MIN_FPS=60.0
if "%MAX_FRAME_TIME%"=="" set MAX_FRAME_TIME=16.67
if "%MAX_MEMORY%"=="" set MAX_MEMORY=1024.0
if "%MAX_BUILD_TIME%"=="" set MAX_BUILD_TIME=10.0
if "%MAX_TEST_TIME%"=="" set MAX_TEST_TIME=5.0
if "%FAIL_ON_WARNINGS%"=="" set FAIL_ON_WARNINGS=false

REM Parse command line arguments
set COMMAND=all
set GATE_NAME=
set SHOW_HELP=false

:parse_args
if "%~1"=="" goto args_parsed
if "%~1"=="--unity-path" (
    set UNITY_PATH=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--project-path" (
    set PROJECT_PATH=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--log-file" (
    set LOG_FILE=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--build-id" (
    set BUILD_ID=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--min-fps" (
    set MIN_FPS=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--max-frame-time" (
    set MAX_FRAME_TIME=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--max-memory" (
    set MAX_MEMORY=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="--fail-on-warnings" (
    set FAIL_ON_WARNINGS=true
    shift
    goto parse_args
)
if "%~1"=="--help" (
    set SHOW_HELP=true
    shift
    goto parse_args
)
if "%~1"=="all" (
    set COMMAND=all
    shift
    goto parse_args
)
if "%~1"=="gate" (
    set COMMAND=gate
    set GATE_NAME=%~2
    shift
    shift
    goto parse_args
)
if "%~1"=="validate" (
    set COMMAND=validate
    shift
    goto parse_args
)
if "%~1"=="report" (
    set COMMAND=report
    shift
    goto parse_args
)
echo [ERROR] Unknown argument: %~1
goto show_usage

:args_parsed

if "%SHOW_HELP%"=="true" goto show_usage

REM Logging functions
:log_info
echo [INFO] %~1
echo [INFO] %~1 >> "%LOG_FILE%"
goto :eof

:log_success
echo [SUCCESS] %~1
echo [SUCCESS] %~1 >> "%LOG_FILE%"
goto :eof

:log_warning
echo [WARNING] %~1
echo [WARNING] %~1 >> "%LOG_FILE%"
goto :eof

:log_error
echo [ERROR] %~1
echo [ERROR] %~1 >> "%LOG_FILE%"
goto :eof

REM Function to check if Unity is available
:check_unity
if not exist "%UNITY_PATH%" (
    call :log_error "Unity not found at: %UNITY_PATH%"
    call :log_error "Please set UNITY_PATH environment variable or install Unity"
    exit /b 1
)
call :log_info "Unity found at: %UNITY_PATH%"
goto :eof

REM Function to validate project structure
:validate_project
if not exist "%PROJECT_PATH%\Assets" (
    call :log_error "Unity project not found at: %PROJECT_PATH%"
    call :log_error "Please set PROJECT_PATH environment variable"
    exit /b 1
)
if not exist "%PROJECT_PATH%\Assets\XRBubbleLibrary\Core\Editor\PerformanceGateCommandLine.cs" (
    call :log_error "Performance gate command line interface not found"
    call :log_error "Please ensure XRBubbleLibrary is properly installed"
    exit /b 1
)
call :log_info "Project structure validated"
goto :eof

REM Function to run Unity command with proper error handling
:run_unity_command
set METHOD=%~1
set ADDITIONAL_ARGS=%~2

call :log_info "Running Unity command: %METHOD%"

REM Create log directory
if not exist "%~dp1" mkdir "%~dp1" 2>nul

REM Run Unity in batch mode
"%UNITY_PATH%" ^
    -batchmode ^
    -quit ^
    -projectPath "%PROJECT_PATH%" ^
    -logFile "%LOG_FILE%" ^
    -executeMethod "%METHOD%" ^
    -buildId "%BUILD_ID%" ^
    -minFPS "%MIN_FPS%" ^
    -maxFrameTime "%MAX_FRAME_TIME%" ^
    -maxMemory "%MAX_MEMORY%" ^
    -maxBuildTime "%MAX_BUILD_TIME%" ^
    -maxTestTime "%MAX_TEST_TIME%" ^
    -failOnWarnings "%FAIL_ON_WARNINGS%" ^
    %ADDITIONAL_ARGS%

set UNITY_EXIT_CODE=%ERRORLEVEL%

if %UNITY_EXIT_CODE%==0 (
    call :log_success "Unity command completed successfully"
    goto :eof
) else if %UNITY_EXIT_CODE%==1 (
    call :log_error "Unity command failed - performance gates blocked the build"
    exit /b 1
) else if %UNITY_EXIT_CODE%==2 (
    call :log_error "Unity command failed - fatal error during execution"
    exit /b 2
) else (
    call :log_error "Unity command failed with unexpected exit code: %UNITY_EXIT_CODE%"
    exit /b %UNITY_EXIT_CODE%
)

REM Function to run all performance gates
:run_all_gates
call :log_info "=== Running All Performance Gates ==="
call :log_info "Build ID: %BUILD_ID%"
call :log_info "Thresholds: FPS>=%MIN_FPS%, FrameTime<=%MAX_FRAME_TIME% ms, Memory<=%MAX_MEMORY% MB"

call :run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.RunAllGatesFromCommandLine"
if %ERRORLEVEL%==0 (
    call :log_success "All performance gates PASSED"
    goto :eof
) else (
    call :log_error "Performance gates FAILED - build blocked"
    exit /b 1
)

REM Function to run a specific gate
:run_specific_gate
if "%GATE_NAME%"=="" (
    call :log_error "Gate name not specified"
    echo Usage: %0 gate "Gate Name"
    exit /b 1
)

call :log_info "=== Running Specific Gate: %GATE_NAME% ==="

call :run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.RunSpecificGateFromCommandLine" "-gateName \"%GATE_NAME%\""
if %ERRORLEVEL%==0 (
    call :log_success "Gate '%GATE_NAME%' PASSED"
    goto :eof
) else (
    call :log_error "Gate '%GATE_NAME%' FAILED"
    exit /b 1
)

REM Function to validate configuration
:validate_configuration
call :log_info "=== Validating Performance Gate Configuration ==="

call :run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.ValidateGateConfigurationFromCommandLine"
if %ERRORLEVEL%==0 (
    call :log_success "Configuration validation PASSED"
    goto :eof
) else (
    call :log_error "Configuration validation FAILED"
    exit /b 1
)

REM Function to generate report
:generate_report
call :log_info "=== Generating Performance Report ==="

call :run_unity_command "XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine.GenerateReportFromCommandLine"
if %ERRORLEVEL%==0 (
    call :log_success "Performance report generated"
    
    REM Check if report path file exists
    set REPORT_PATH_FILE=%PROJECT_PATH%\..\performance_report_path.txt
    if exist "!REPORT_PATH_FILE!" (
        set /p REPORT_PATH=<"!REPORT_PATH_FILE!"
        call :log_info "Report available at: !REPORT_PATH!"
        
        REM Copy report to CI artifacts directory if specified
        if not "%CI_ARTIFACTS_DIR%"=="" (
            if not exist "%CI_ARTIFACTS_DIR%" mkdir "%CI_ARTIFACTS_DIR%"
            copy "!REPORT_PATH!" "%CI_ARTIFACTS_DIR%\" >nul
            call :log_info "Report copied to CI artifacts: %CI_ARTIFACTS_DIR%"
        )
    )
    goto :eof
) else (
    call :log_error "Report generation FAILED"
    exit /b 1
)

REM Function to display usage
:show_usage
echo Usage: %0 [OPTIONS] [COMMAND]
echo.
echo Commands:
echo   all                    Run all performance gates (default)
echo   gate ^<name^>           Run specific performance gate
echo   validate              Validate gate configuration only
echo   report                Generate performance report only
echo.
echo Options:
echo   --unity-path PATH     Path to Unity executable
echo   --project-path PATH   Path to Unity project
echo   --log-file FILE       Path to log file
echo   --build-id ID         Build identifier
echo   --min-fps FPS         Minimum FPS threshold (default: 60.0)
echo   --max-frame-time MS   Maximum frame time in ms (default: 16.67)
echo   --max-memory MB       Maximum memory usage in MB (default: 1024.0)
echo   --fail-on-warnings    Fail build on warnings (default: false)
echo   --help                Show this help message
echo.
echo Environment Variables:
echo   UNITY_PATH            Path to Unity executable
echo   PROJECT_PATH          Path to Unity project
echo   BUILD_ID              Build identifier
echo   CI_ARTIFACTS_DIR      Directory to copy reports for CI artifacts
echo.
echo Examples:
echo   %0                                    # Run all gates
echo   %0 gate "Unit Tests"                 # Run specific gate
echo   %0 validate                          # Validate configuration
echo   %0 --min-fps 72 --max-memory 512     # Custom thresholds
exit /b 0

REM Main execution
call :log_info "=== Performance Gates CI/CD Script ==="
call :log_info "Command: %COMMAND%"
call :log_info "Build ID: %BUILD_ID%"
call :log_info "Project Path: %PROJECT_PATH%"

REM Validate environment
call :check_unity
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :validate_project
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

REM Execute command
if "%COMMAND%"=="all" (
    call :run_all_gates
    if %ERRORLEVEL%==0 (
        call :log_success "=== ALL PERFORMANCE GATES PASSED ==="
        exit /b 0
    ) else (
        call :log_error "=== PERFORMANCE GATES FAILED - BUILD BLOCKED ==="
        exit /b 1
    )
) else if "%COMMAND%"=="gate" (
    call :run_specific_gate
    if %ERRORLEVEL%==0 (
        call :log_success "=== GATE PASSED ==="
        exit /b 0
    ) else (
        call :log_error "=== GATE FAILED ==="
        exit /b 1
    )
) else if "%COMMAND%"=="validate" (
    call :validate_configuration
    if %ERRORLEVEL%==0 (
        call :log_success "=== CONFIGURATION VALID ==="
        exit /b 0
    ) else (
        call :log_error "=== CONFIGURATION INVALID ==="
        exit /b 1
    )
) else if "%COMMAND%"=="report" (
    call :generate_report
    if %ERRORLEVEL%==0 (
        call :log_success "=== REPORT GENERATED ==="
        exit /b 0
    ) else (
        call :log_error "=== REPORT GENERATION FAILED ==="
        exit /b 1
    )
) else (
    call :log_error "Unknown command: %COMMAND%"
    goto show_usage
)