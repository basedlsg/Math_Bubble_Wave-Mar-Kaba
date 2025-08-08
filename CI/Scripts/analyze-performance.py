#!/usr/bin/env python3
"""
Performance Analysis Script for CI/CD Pipeline
Analyzes performance gate results and generates reports and visualizations.
Implements Requirement 4.6: CI/CD pipeline integration with comprehensive reporting.
"""

import os
import sys
import json
import csv
import glob
import argparse
from datetime import datetime
from pathlib import Path
import re

def parse_log_file(log_path):
    """Parse Unity log file to extract performance gate results."""
    results = {
        'gates': {},
        'overall_success': False,
        'execution_time': 0,
        'build_id': 'unknown',
        'timestamp': None
    }
    
    try:
        with open(log_path, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
            
        # Extract build ID
        build_id_match = re.search(r'Build ID: ([^\s\n]+)', content)
        if build_id_match:
            results['build_id'] = build_id_match.group(1)
        
        # Extract overall result
        if 'ALL PERFORMANCE GATES PASSED' in content:
            results['overall_success'] = True
        elif 'PERFORMANCE GATES FAILED' in content:
            results['overall_success'] = False
        
        # Extract individual gate results
        gate_pattern = r'\[(\w+)\] Gate \'([^\']+)\': (PASS|FAIL)'
        for match in re.finditer(gate_pattern, content):
            gate_name = match.group(2)
            gate_result = match.group(3) == 'PASS'
            results['gates'][gate_name] = gate_result
        
        # Extract performance metrics
        metrics_pattern = r'(\w+): ([\d.]+)'
        metrics = {}
        for match in re.finditer(metrics_pattern, content):
            metric_name = match.group(1)
            metric_value = float(match.group(2))
            metrics[metric_name] = metric_value
        
        results['metrics'] = metrics
        
    except Exception as e:
        print(f"Error parsing log file {log_path}: {e}")
    
    return results

def parse_csv_report(csv_path):
    """Parse CSV performance report."""
    data = []
    try:
        with open(csv_path, 'r') as f:
            reader = csv.DictReader(f)
            for row in reader:
                # Convert numeric fields
                for key, value in row.items():
                    try:
                        row[key] = float(value)
                    except (ValueError, TypeError):
                        pass  # Keep as string
                data.append(row)
    except Exception as e:
        print(f"Error parsing CSV file {csv_path}: {e}")
    
    return data

def parse_json_report(json_path):
    """Parse JSON performance report."""
    try:
        with open(json_path, 'r') as f:
            return json.load(f)
    except Exception as e:
        print(f"Error parsing JSON file {json_path}: {e}")
        return {}

def analyze_artifacts_directory(artifacts_dir):
    """Analyze all performance artifacts in the directory."""
    artifacts_path = Path(artifacts_dir)
    analysis = {
        'summary': {
            'total_gates': 0,
            'passed_gates': 0,
            'failed_gates': 0,
            'overall_success': False,
            'analysis_timestamp': datetime.utcnow().isoformat()
        },
        'gates': {},
        'performance_data': {},
        'reports': []
    }
    
    print(f"Analyzing artifacts in: {artifacts_dir}")
    
    # Find and parse log files
    log_files = list(artifacts_path.glob('**/performance-gates.log'))
    log_files.extend(list(artifacts_path.glob('**/*.log')))
    
    for log_file in log_files:
        print(f"Parsing log file: {log_file}")
        log_results = parse_log_file(log_file)
        
        # Merge gate results
        for gate_name, gate_result in log_results['gates'].items():
            analysis['gates'][gate_name] = gate_result
            analysis['summary']['total_gates'] += 1
            if gate_result:
                analysis['summary']['passed_gates'] += 1
            else:
                analysis['summary']['failed_gates'] += 1
        
        # Update overall success
        if log_results['overall_success']:
            analysis['summary']['overall_success'] = True
    
    # Find and parse CSV reports
    csv_files = list(artifacts_path.glob('**/*.csv'))
    for csv_file in csv_files:
        print(f"Parsing CSV file: {csv_file}")
        csv_data = parse_csv_report(csv_file)
        if csv_data:
            report_name = csv_file.stem
            analysis['performance_data'][report_name] = csv_data
    
    # Find and parse JSON reports
    json_files = list(artifacts_path.glob('**/*.json'))
    for json_file in json_files:
        print(f"Parsing JSON file: {json_file}")
        json_data = parse_json_report(json_file)
        if json_data:
            analysis['reports'].append({
                'name': json_file.stem,
                'path': str(json_file),
                'data': json_data
            })
    
    return analysis

def generate_summary_report(analysis, output_dir):
    """Generate a summary report in markdown format."""
    output_path = Path(output_dir) / 'performance-analysis-summary.md'
    
    with open(output_path, 'w') as f:
        f.write("# Performance Gates Analysis Summary\n\n")
        f.write(f"**Analysis Timestamp**: {analysis['summary']['analysis_timestamp']}\n\n")
        
        # Overall status
        status_emoji = "✅" if analysis['summary']['overall_success'] else "❌"
        status_text = "PASSED" if analysis['summary']['overall_success'] else "FAILED"
        f.write(f"## Overall Status: {status_emoji} {status_text}\n\n")
        
        # Summary statistics
        f.write("## Summary Statistics\n\n")
        f.write(f"- **Total Gates**: {analysis['summary']['total_gates']}\n")
        f.write(f"- **Passed Gates**: {analysis['summary']['passed_gates']}\n")
        f.write(f"- **Failed Gates**: {analysis['summary']['failed_gates']}\n")
        
        if analysis['summary']['total_gates'] > 0:
            success_rate = (analysis['summary']['passed_gates'] / analysis['summary']['total_gates']) * 100
            f.write(f"- **Success Rate**: {success_rate:.1f}%\n")
        
        f.write("\n")
        
        # Individual gate results
        if analysis['gates']:
            f.write("## Individual Gate Results\n\n")
            f.write("| Gate Name | Status |\n")
            f.write("|-----------|--------|\n")
            
            for gate_name, gate_result in analysis['gates'].items():
                status_emoji = "✅" if gate_result else "❌"
                status_text = "PASS" if gate_result else "FAIL"
                f.write(f"| {gate_name} | {status_emoji} {status_text} |\n")
            
            f.write("\n")
        
        # Performance data summary
        if analysis['performance_data']:
            f.write("## Performance Data Summary\n\n")
            
            for report_name, data in analysis['performance_data'].items():
                f.write(f"### {report_name}\n\n")
                
                if data and isinstance(data, list) and len(data) > 0:
                    # Calculate basic statistics for numeric columns
                    numeric_columns = []
                    for key, value in data[0].items():
                        if isinstance(value, (int, float)):
                            numeric_columns.append(key)
                    
                    if numeric_columns:
                        f.write("| Metric | Min | Max | Average |\n")
                        f.write("|--------|-----|-----|----------|\n")
                        
                        for col in numeric_columns:
                            values = [row[col] for row in data if isinstance(row.get(col), (int, float))]
                            if values:
                                min_val = min(values)
                                max_val = max(values)
                                avg_val = sum(values) / len(values)
                                f.write(f"| {col} | {min_val:.2f} | {max_val:.2f} | {avg_val:.2f} |\n")
                        
                        f.write("\n")
                
                f.write(f"**Data Points**: {len(data)}\n\n")
        
        # Reports summary
        if analysis['reports']:
            f.write("## Available Reports\n\n")
            
            for report in analysis['reports']:
                f.write(f"- **{report['name']}**: {report['path']}\n")
            
            f.write("\n")
        
        # Recommendations
        f.write("## Recommendations\n\n")
        
        if analysis['summary']['failed_gates'] > 0:
            f.write("❌ **Action Required**: Some performance gates have failed.\n\n")
            f.write("**Failed Gates:**\n")
            for gate_name, gate_result in analysis['gates'].items():
                if not gate_result:
                    f.write(f"- {gate_name}\n")
            f.write("\n")
            f.write("**Next Steps:**\n")
            f.write("1. Review the detailed logs for each failed gate\n")
            f.write("2. Address the specific issues identified\n")
            f.write("3. Re-run the performance gates\n")
            f.write("4. Consider adjusting performance thresholds if appropriate\n")
        else:
            f.write("✅ **All Clear**: All performance gates have passed successfully.\n\n")
            f.write("**Next Steps:**\n")
            f.write("1. Proceed with build deployment\n")
            f.write("2. Monitor performance metrics in production\n")
            f.write("3. Consider tightening performance thresholds for continuous improvement\n")
    
    print(f"Summary report generated: {output_path}")
    return str(output_path)

def generate_detailed_analysis(analysis, output_dir):
    """Generate detailed analysis files."""
    output_dir = Path(output_dir)
    output_dir.mkdir(exist_ok=True)
    
    # Save full analysis as JSON
    analysis_json_path = output_dir / 'full-analysis.json'
    with open(analysis_json_path, 'w') as f:
        json.dump(analysis, f, indent=2, default=str)
    
    print(f"Detailed analysis saved: {analysis_json_path}")
    
    # Generate CSV summary of gate results
    if analysis['gates']:
        gates_csv_path = output_dir / 'gate-results.csv'
        with open(gates_csv_path, 'w', newline='') as f:
            writer = csv.writer(f)
            writer.writerow(['Gate Name', 'Status', 'Success'])
            
            for gate_name, gate_result in analysis['gates'].items():
                status = 'PASS' if gate_result else 'FAIL'
                writer.writerow([gate_name, status, gate_result])
        
        print(f"Gate results CSV saved: {gates_csv_path}")

def main():
    """Main entry point for the performance analysis script."""
    parser = argparse.ArgumentParser(description='Analyze performance gate results')
    parser.add_argument('artifacts_dir', help='Directory containing performance artifacts')
    parser.add_argument('--output-dir', default='performance-analysis', help='Output directory for analysis results')
    parser.add_argument('--verbose', '-v', action='store_true', help='Enable verbose output')
    
    args = parser.parse_args()
    
    if not os.path.exists(args.artifacts_dir):
        print(f"Error: Artifacts directory not found: {args.artifacts_dir}")
        sys.exit(1)
    
    # Create output directory
    output_dir = Path(args.output_dir)
    output_dir.mkdir(exist_ok=True)
    
    print("=== Performance Gates Analysis ===")
    print(f"Artifacts Directory: {args.artifacts_dir}")
    print(f"Output Directory: {args.output_dir}")
    print()
    
    # Analyze artifacts
    analysis = analyze_artifacts_directory(args.artifacts_dir)
    
    # Generate reports
    summary_path = generate_summary_report(analysis, args.output_dir)
    generate_detailed_analysis(analysis, args.output_dir)
    
    # Print summary to console
    print("\n=== Analysis Summary ===")
    print(f"Total Gates: {analysis['summary']['total_gates']}")
    print(f"Passed Gates: {analysis['summary']['passed_gates']}")
    print(f"Failed Gates: {analysis['summary']['failed_gates']}")
    
    if analysis['summary']['total_gates'] > 0:
        success_rate = (analysis['summary']['passed_gates'] / analysis['summary']['total_gates']) * 100
        print(f"Success Rate: {success_rate:.1f}%")
    
    overall_status = "PASSED" if analysis['summary']['overall_success'] else "FAILED"
    print(f"Overall Status: {overall_status}")
    
    print(f"\nDetailed reports available in: {args.output_dir}")
    
    # Exit with appropriate code
    if analysis['summary']['failed_gates'] > 0:
        print("\n❌ Some performance gates failed - check the detailed reports")
        sys.exit(1)
    else:
        print("\n✅ All performance gates passed successfully")
        sys.exit(0)

if __name__ == '__main__':
    main()