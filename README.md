# RunTI

## Overview

Add context menu options for system administration to use the [RunTI](/src/) open-source command-line utility to run as **TrustedInstaller**.

> [!WARNING]  
> Adding these context menu options allows you to modify system behavior and potentially cause harm if used incorrectly.

## Installation

This script will install `RunTI` to the Windows root directory and add context menu options:

Visible with the **Shift** key:
```powershell
PowerShell -ExecutionPolicy Unrestricted -Command "iwr https://raw.githubusercontent.com/edelvarden/RunTI/main/install_with_shift.ps1 -useb | iex"
```

Visible directly:
```powershell
PowerShell -ExecutionPolicy Unrestricted -Command "iwr https://raw.githubusercontent.com/edelvarden/RunTI/main/install.ps1 -useb | iex"
```

This script will **remove** `RunTI` from the Windows root directory and the context menu options:
```powershell
PowerShell -ExecutionPolicy Unrestricted -Command "iwr https://raw.githubusercontent.com/edelvarden/RunTI/main/remove.ps1 -useb | iex"
```

## Manual Installation

1. Download `RunTI.exe` from [releases](https://github.com/edelvarden/RunTI/releases/latest).
2. Move it to the Windows root directory `C:\Windows`.
3. To add context menu options, **Merge** the appropriate registry file:
   - `run_as_trustedinstaller_with_shift.reg` visible only when with the **Shift** key,
   - `run_as_trustedinstaller.reg` to see them without.

To remove these options, merge `remove_run_as_trustedinstaller.reg`.

## Options

| Option                          | Description                                                                                     |
| -------------------------------- | ----------------------------------------------------------------------------------------------- |
| Run as TrustedInstaller         | Adds this option for `.exe`, `.bat`, `.cmd`, and `.ps1` files to open with elevated privileges. |
| Merge as TrustedInstaller       | Adds this option for `.reg` files to merge into system-protected directories. Use with caution. |
| Open in CMD as TrustedInstaller | Adds this option to folder and folder background context menus to open the folder path in CMD. |
| Network                          | Adds this option to the desktop background context menu to enable or disable the network connection. |
| Destroy                          | Adds this option for folders and files to be permanently deleted without moving to the Recycle Bin. |