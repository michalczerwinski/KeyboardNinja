# Copilot Instructions

## Project Guidelines
- User uses NativeMethods.txt with CsWin32 (Microsoft.Windows.CsWin32) source generator for Win32 API P/Invoke calls. All Win32 functions should be declared in NativeMethods.txt and accessed via PInvoke.* rather than manual DllImport declarations.
- Use Windows Forms (System.Windows.Forms) for the UI. Avoid WPF or other frameworks.
- Follow standard C# coding conventions and best practices. Use meaningful variable and method names, and include XML documentation comments for public members.
- 