# WinHack (Ongoing)
WinHack is a library (or perhaps framework?) designed to make Windows user-space manipulation easier for .NET developers.

It provides foundation and abstractions for working with process manipulation, memory access, and API hooking. Instead of writing repetitive boilerplate code, developers can focus directly on their experiments.

## Target
WinHack currently targets:

- Windows 11 and Windows 10 (2004+)
- 64-bit (no intention on supporting 32-bit, since nearly all current machines run 64-bit)
- .NET 8 (With possible support for lower versions in the future up to .NET 6)


## Features
- Utilities for working with window/monitor
- Local/Global window hook (Mouse, Keyboard, CallWnd, etc) with support for hooking 32-bit process.
- Code hook and detouring
- GDI manipulation
- 
