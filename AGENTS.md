# AGENTS

## Platform

- Pompom is a Windows-only WPF application.
- The repository can be in WSL, but builds and tests must use Windows executables.
- Do not use a Linux .NET SDK for this project.

## .NET SDK

- `global.json` requires .NET SDK 10.0.401.
- Make sure that `dotnet.exe` resolves to a Windows SDK that satisfies `global.json`.
- Run `dotnet.exe --version` before a build if more than one Windows SDK installation is available.

## Build and test from WSL

Run the following commands from the repository root:

```sh
dotnet.exe build Pompom.sln --nologo
dotnet.exe tests/Pompom.Tests/bin/Debug/net10.0-windows10.0.17763.0/win-x64/Pompom.Tests.dll
```

Do not rely on `dotnet test` while the repository is on the WSL filesystem. The
.NET test runner can build the test project but report that it ran zero tests.
Run the built test assembly directly, as shown above.

## Windows behavior

- Unit tests can validate notification XML, but they cannot validate Windows notification behavior.
- For notification changes, use **Send test notification** and do a manual check on Windows.
- For an alarm, make sure that the sound continues until you select **Dismiss**.
