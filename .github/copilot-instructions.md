# Log Insights for One Identity Manager

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

Log Insights is a .NET 8 Windows Forms desktop application that analyzes One Identity Manager log files and Azure Application Insights logs to provide actionable insights over system behavior. The application helps administrators identify patterns, performance issues, and system behaviors hidden in complex log data.

## Working Effectively

### Prerequisites and Platform Requirements
- **CRITICAL**: This application requires Windows to build and run due to Windows Forms dependencies.
- Download and install the [.NET 8 SDK](https://dotnet.microsoft.com/download).
- The application cannot be built or run on Linux/macOS environments.
- Visual Studio or Visual Studio Code on Windows is recommended for development.

### Building and Running
1. **Initial Setup**:
   ```powershell
   git clone https://github.com/OneIdentity/IdentityManager.LogInsights.git
   cd IdentityManager.LogInsights
   ```

2. **Build the Application**:
   ```powershell
   dotnet build
   ```
   - **NEVER CANCEL**: Build typically takes 2-5 minutes. ALWAYS set timeout to 10+ minutes.
   - Build output: `LogInsights/bin/Debug/net8.0-windows/`

3. **Run the Application**:
   ```powershell
   dotnet run --project LogInsights\LogInsights.csproj
   ```
   - **NEVER CANCEL**: Application startup takes 10-30 seconds. Set timeout to 2+ minutes.
   - Alternative: Open `LogInsights.sln` in Visual Studio and run from there.

### Project Structure
- **LogInsights.sln**: Main solution file
- **LogInsights/**: Main application project
  - **LogInsights.csproj**: Project file with .NET 8 Windows Forms target
  - **Program.cs**: Application entry point
  - **MainForm.cs**: Main UI form and application logic
  - **Detectors/**: Log analysis components for different log types
  - **LogReader/**: Implementations for reading different log sources
  - **Controls/**: Custom UI controls and components
  - **Helpers/**: Utility classes and extension methods
  - **Datastore/**: Data management and storage components

## Validation and Testing

### Manual Validation Requirements
- **CRITICAL**: This application has NO automated tests. ALL validation must be done manually.
- After making changes, ALWAYS build and run the application to verify functionality.
- **Essential Test Scenarios**:
  1. **Basic Application Launch**: Verify the application starts without errors
  2. **File Loading**: Test drag-and-drop of log files onto the left tree view
  3. **Log Analysis**: Open sample log files and verify detectors run successfully
  4. **UI Responsiveness**: Ensure all controls and menus function properly

### CI/CD and Build Validation
- The CI pipeline runs on Windows runners using MSBuild.
- **No linting commands exist** - the project uses standard .NET analyzers.
- **No test commands exist** - manual testing is required.
- Always verify your changes don't break the CI by checking that `dotnet build` succeeds.

## Key Application Components

### Detectors (LogInsights/Detectors/)
Core analysis components that process log data:
- **TimeRangeDetector**: Analyzes time spans across log files
- **SyncStructureDetector**: Detects Identity Manager synchronization processes
- **ConnectorsDetector**: Identifies connector activity and relationships
- **SQLStatementDetector**: Analyzes database queries and performance
- **JobServiceJobsDetector**: Tracks JobService process execution
- **TimeGapDetector**: Identifies potential deadlock situations

**Detector Dependencies** (from _Dependencies.txt):
```
TimeRangeDetector
├── SyncStructureDetector
│   ├── ConnectorsDetector
│   │   ├── SyncStepDetailsDetector
│   │   └── SyncJournalDetector
│   └── SQLStatementDetector
└── TimeGapDetector
```

### LogReader Implementations (LogInsights/LogReader/)
Handle different log data sources:
- **Local log files**: Standard NLog format files
- **Azure Application Insights**: Cloud-based log queries
- **JobService logs**: Specialized One Identity Manager job logs

### Main UI Components
- **MainForm.cs**: Primary application window and orchestration
- **ContextLinesUC**: Displays log context around selected entries
- **ListViewUC/MultiListViewUC**: Custom list controls for log data
- **TimeTraceUC**: Timeline visualization of log events
- **WelcomeUC**: Initial user interface and guidance

## Common Development Tasks

### Adding New Log Detectors
1. Create new detector class in `LogInsights/Detectors/`
2. Inherit from `DetectorBase` or implement `ILogDetector`
3. Update `_Dependencies.txt` if detector has dependencies
4. Register detector in the main analysis pipeline
5. **ALWAYS** test with sample log files after implementation

### Modifying UI Components
1. Windows Forms controls are in `LogInsights/Controls/`
2. Use Visual Studio designer for layout changes when possible
3. **ALWAYS** test UI changes by running the application
4. Check responsiveness across different window sizes

### Working with Log Formats
1. Log parsing logic is in `LogReader/` implementations
2. Regular expressions for log patterns are in `GlobalDefs/Constants.cs`
3. **CRITICAL**: Test log parsing with various real-world log samples

## Important Code Locations

### Frequently Referenced Files
- **Constants.cs**: Global configuration, regex patterns, UI colors
- **MainForm.cs**: Application orchestration and UI event handling
- **DetectorBase.cs**: Base class for all log analysis components
- **LogEntry.cs**: Core data structure for log entries
- **ExceptionHandler.cs**: Centralized error handling

### Configuration Files
- **.editorconfig**: C# code style rules and formatting
- **nlog.config**: Application logging configuration
- **.github/workflows/ci.yml**: Build and validation pipeline

## Development Guidelines

### Code Style and Standards
- Follow the .editorconfig rules (spaces for indentation, explicit types over var)
- Use meaningful variable names and clear method signatures
- Add exception handling for all user-facing operations
- **NEVER** remove existing functionality without thorough testing

### Performance Considerations
- Log files can be very large (GB+) - always consider memory usage
- Use streaming operations for large file processing
- Implement progress reporting for long-running operations
- Profile performance when working with new detector implementations

### Error Handling
- All UI operations should include try-catch blocks
- Use `ExceptionHandler.Instance.HandleException()` for consistent error reporting
- Provide meaningful error messages to users
- Log errors for debugging purposes

## Platform-Specific Notes
- **Windows-only**: Cannot build or run on Linux/macOS
- **Windows Forms**: UI framework requires Windows desktop environment
- **File paths**: Use Windows path separators in examples (`\` not `/`)
- **PowerShell**: Recommended command shell for Windows development

Remember: This is a Windows-only desktop application with no automated tests. Always validate changes by running the application and testing core functionality manually.