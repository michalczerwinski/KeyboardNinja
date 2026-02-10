# KeyboardNinja

A Windows keyboard customization tool that allows you to remap keys and create custom keyboard shortcuts.

## Features

- **Key Remapping**: Remap any key to another key or combination
- **Custom Shortcuts**: Create powerful keyboard shortcuts for common tasks
- **Multiple Mapping Profiles**: Organize shortcuts by category (Clipboard, Navigation, Selection, Window Management, etc.)
- **Low-Level Keyboard Hook**: Intercepts keyboard input at system level for reliable operation
- **Native Windows Integration**: Uses Windows APIs via CsWin32 for optimal performance

## Requirements

- Windows 10 or later
- .NET 10.0

## Building

```bash
dotnet restore
dotnet build
```

## Running

```bash
dotnet run
```

Or run the compiled executable from `bin\Release\net10.0-windows\KeyboardNinja.exe`

## Project Structure

- `src/` - Source code
  - `Helpers/` - Utility classes (Desktop, Form, Icon, Toast Notifications)
  - `Mappings/` - Mapping configuration files organized by category
  - Forms and main application logic
- `KeyboardNinja.csproj` - Project file

## Dependencies

- **Microsoft.Windows.CsWin32** - Source generator for Win32 API P/Invoke
- **SharpHook** - Cross-platform keyboard and mouse hooking library

## License

MIT License - see the [LICENSE](LICENSE) file for details

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.
