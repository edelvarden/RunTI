# RunTI

## Overview

Add context menu options for system administration to use the [RunTI](/src/) - open-source command-line utility to run as **TrustedInstaller**.

## ⚠️ WARNING: Use with caution!

Adding these context menu options allows you to modify system behavior and potentially cause harm if used incorrectly.

## Installation

1. Download `RunTI.exe` from [releases](https://github.com/edelvarden/RunTI/releases/latest).
2. Move it to the Windows root directory `C:\Windows`.
3. To add context menu options, **Merge** the appropriate variant:
   - `run_as_trustedinstaller_with_shift.reg` visible only with the shift key,
   - `run_as_trustedinstaller.reg` to see them without.

To remove these options, merge `remove_run_as_trustedinstaller.reg`.

## Options

| Option                          | Description                                                                                     |
| ------------------------------- | ----------------------------------------------------------------------------------------------- |
| Run as TrustedInstaller         | Adds this option for `.exe`, `.bat`, `.cmd`, and `.ps1` files to open with elevated privileges. |
| Merge as TrustedInstaller       | Adds this option for `.reg` files to merge into system-protected directories. Use with caution. |
| Open in CMD as TrustedInstaller | Adds this option for folder and folder background context menus to open folder path in cmd.     |
