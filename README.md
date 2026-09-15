# Pompom

Pompom is a small Pomodoro timer for Windows. It provides a focused timer, a
system tray icon, global keyboard shortcuts, and Windows notifications.

## Features

- Set the work, short-break, and long-break duration from 1 through 180 minutes.
- Enable or disable the long break after four work sessions.
- Start breaks and work sessions automatically with separate settings.
- Start, stop, skip, or reset the current session.
- Control the timer with global keyboard shortcuts.
- Hide the window in the system tray when you close or minimize it.
- Show native Windows notifications in Notification Center.
- Play short, distinct Windows sounds when a break starts and when work starts.
- Use a tray balloon when native notifications are not available.
- Enable work and break notifications separately.
- Keep only one Pompom process open. A second launch restores the first window.

Pompom does not save timer progress. It starts with a new work session after you
exit and open it again.

## Keyboard shortcuts

| Action | Shortcut |
| --- | --- |
| Start or resume | `Ctrl+Alt+P` |
| Stop and keep the remaining time | `Ctrl+Alt+X` |
| Skip to the next session | `Ctrl+Alt+N` |
| Reset the current session | `Ctrl+Alt+R` |
| Show or hide the window | `Ctrl+Alt+M` |

The shortcuts work while Pompom is in the system tray. If another application
uses a shortcut, Pompom shows a warning and continues without that shortcut.

## Run the portable build

Pompom supports 64-bit Windows 10 version 1809 or later and Windows 11.

1. Extract `Pompom-win-x64.zip` to a local folder.
2. Run `Pompom.exe`.
3. Use the tray menu and select **Exit** when you want to close the process.

The portable build includes the required .NET and Windows App SDK files. You do
not need to install a runtime.

Pompom stores settings in `%LOCALAPPDATA%\Pompom\settings.json`.

## Build and test

Install the .NET 10 SDK on Windows. You can install it with Scoop:

```powershell
scoop install dotnet-sdk
```

Then run these commands from a Windows PowerShell terminal:

```powershell
dotnet restore Pompom.sln
dotnet build Pompom.sln --no-restore
dotnet test Pompom.sln --no-restore
```

The repository pins SDK `10.0.401`. A later patch of that SDK also works.

If the repository is in WSL, run the commands with the Windows `dotnet.exe`.
Map the WSL path to a drive first. Windows manifest tools cannot process a UNC
path directly.

```cmd
pushd \\wsl.localhost\Arch\home\user\code\pompom
dotnet build Pompom.sln
```

Replace `Arch` and the repository path with your WSL distribution and path.

## Create the portable package

Run the publish script from Windows PowerShell:

```powershell
.\scripts\publish.ps1
```

The script publishes on the Windows local temporary drive first. It then copies
the completed folder and archive to `dist`. This avoids .NET copy failures on
the WSL network share. For a repository on a WSL share, WSL copies the final
files. Windows does not modify `dist` through the network share.

The script creates these outputs:

- `dist\Pompom-win-x64\`
- `dist\Pompom-win-x64.zip`

The package is a folder instead of a single executable because the native
Windows notification components require supporting files.

## Notification troubleshooting

- Open **Settings > System > Notifications** and make sure notifications are on.
- Turn on notifications for Pompom after Windows adds it to the application list.
- Use **Send test notification** in Pompom settings.
- Focus Assist or Do Not Disturb can hide a banner. The notification can still
  appear in Notification Center.
- Pompom uses a tray balloon if native Windows notifications are not supported or
  cannot start.
