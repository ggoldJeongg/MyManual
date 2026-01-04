# AGENTS.md

This file contains build commands and code style guidelines for the MyManual WPF onboarding application.

## Build Commands

### Basic Build
```bash
dotnet build
```

### Build and Run
```bash
dotnet run
```

### Clean Build
```bash
dotnet clean && dotnet build
```

### Build for Release
```bash
dotnet build -c Release
```

### Testing
This project currently does not have automated tests. When adding tests:
```bash
dotnet test
```

### Linting
This project uses built-in .NET compiler warnings and nullable reference types. No external linter is configured.

## Code Style Guidelines

### Project Structure
- **RootNamespace**: `MyManual`
- **AssemblyName**: `MyManual`
- **Target Framework**: `net8.0-windows`
- **Nullable**: Enabled
- **ImplicitUsings**: Enabled

### Namespace Organization
```
MyManual/
├── Commands/          # ICommand implementations
├── Helpers/           # Utility classes and static helpers
├── Models/            # Data models
│   ├── User/         # User-related models
│   └── Onboarding/   # Onboarding-related models
├── ViewModels/       # MVVM ViewModels
│   └── Base/         # Base classes
└── Views/            # WPF Views and code-behind
```

### Import Organization
1. **System namespaces** (alphabetical)
2. **Microsoft namespaces** (alphabetical) 
3. **Third-party namespaces** (alphabetical)
4. **Project namespaces** (alphabetical)

Example:
```csharp
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using MyManual.Commands;
using MyManual.Models.User;
using MyManual.ViewModels.Base;
```

### Naming Conventions
- **Classes**: `PascalCase` (e.g., `OnboardingViewModel`, `RelayCommand`)
- **Interfaces**: `PascalCase` with `I` prefix (e.g., `ICommand`)
- **Methods**: `PascalCase` (e.g., `LoadTasksForCurrentDay`, `OnToggleTask`)
- **Properties**: `PascalCase` (e.g., `CurrentDay`, `IsCompleted`)
- **Fields**: `_camelCase` with underscore prefix (e.g., `_currentDay`, `_isCompleted`)
- **Constants**: `PascalCase` (e.g., `MaximumDays`)
- **Events**: `PascalCase` (e.g., `PropertyChanged`, `CanExecuteChanged`)

### MVVM Pattern Guidelines
- **ViewModels**: Inherit from `ViewModelBase` for property change notification
- **Models**: Should be plain data objects, avoid UI dependencies
- **Commands**: Use `RelayCommand` for command implementation
- **Property Binding**: Use `SetProperty()` method from `ViewModelBase`

### Property Implementation
```csharp
private int _currentDay;
public int CurrentDay
{
    get => _currentDay;
    set
    {
        SetProperty(ref _currentDay, value);
        // Additional logic if needed
        LoadTasksForCurrentDay();
    }
}
```

### Command Implementation
```csharp
public ICommand ToggleTaskCommand { get; }

// In constructor:
ToggleTaskCommand = new RelayCommand(OnToggleTask, CanToggleTask);

private void OnToggleTask(object? parameter)
{
    // Command execution logic
}

private bool CanToggleTask(object? parameter)
{
    // Command can execute logic
    return parameter is OnboardingTask;
}
```

### Error Handling
- Use nullable reference types (`string?`, `object?`) where appropriate
- Validate parameters in constructors and methods
- Use `ArgumentNullException.ThrowIfNull()` for required parameters
- Handle command parameter type checking with `is` pattern matching

### Comments and Documentation
- **Korean comments are used** in this codebase (as seen in existing code)
- Add comments for complex business logic
- Document command purposes and parameter expectations
- Use region comments for code organization:
```csharp
// ==================== 데이터 ====================
// ==================== Commands ====================
// ==================== 생성자 ====================
// ==================== 메서드 ====================
```

### XAML Guidelines
- Use consistent naming: `{Component}View.xaml` and `{Component}View.xaml.cs`
- Organize layout with proper Grid/StackPanel structure
- Use consistent color scheme: Primary `#00A878`, Background `#F5F5F5`
- Bind commands using `RelativeSource={RelativeSource AncestorType=Window}`
- Use proper data templates for list items

### File Organization
- Keep related classes in appropriate folders
- Use partial classes only for designer-generated code
- Avoid putting multiple classes in one file unless they are small and closely related

### Type Safety
- Leverage nullable reference types
- Use generic types appropriately (`ObservableCollection<T>`, `List<T>`)
- Prefer strongly-typed parameters over `object?` when possible
- Use pattern matching for type checking

### Performance Considerations
- Use `ObservableCollection<T>` for data binding to lists
- Implement `INotifyPropertyChanged` efficiently via `ViewModelBase`
- Avoid expensive operations in property getters
- Use command parameter binding instead of string-based command names

## Development Workflow
1. Build the project to ensure compilation
2. Follow the existing MVVM pattern when adding new features
3. Use Korean comments to match existing codebase style
4. Test UI interactions manually (no automated tests currently)
5. Ensure proper cleanup of event handlers and resources

## Common Patterns
- **Property Change Notification**: Use `ViewModelBase.SetProperty()`
- **Command Binding**: Use `RelayCommand` with parameter validation
- **Data Loading**: Load data in ViewModels, not Views
- **UI Updates**: Use property change notification, not direct UI manipulation