# Repository Guidelines

## Project Structure & Module Organization
- `MmLogView.sln` and `MmLogView.csproj` define a single WPF desktop app targeting `net10.0-windows`.
- UI entry points: `App.xaml`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `GoToLineDialog.cs`.
- Domain logic lives in `Core/` (file mapping, line index, recent files), UI behavior in `ViewModels/`, custom rendering in `Controls/`, value converters in `Converters/`.
- Themes and localization assets are in `Themes/` and `Localization/`.
- Build outputs are `bin/` and `obj/`; do not edit generated files.

## Build, Test, and Development Commands
- Restore and build (Debug): `dotnet build MmLogView.sln`
- Run locally: `dotnet run --project MmLogView.csproj`
- Release build: `dotnet build MmLogView.csproj -c Release`
- Publish single-file `win-x64` package:  
  `dotnet publish MmLogView.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true`
- Optional cleanup before reproducible builds: `dotnet clean MmLogView.sln`

## Coding Style & Naming Conventions
- C# uses nullable reference types and implicit usings (`<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`).
- Use 4-space indentation; UTF-8 encoding for all source files.
- Naming: `PascalCase` for types/methods/properties, `_camelCase` for private fields, descriptive XAML `x:Name` values (for example `RecentDropdownBtn`).
- Keep UI logic in ViewModels when possible; avoid duplicating file/line-processing logic already in `Core/`.

## Testing Guidelines
- No dedicated test project is currently present.
- Minimum verification for changes: run `dotnet build` and smoke-test via `dotnet run`.
- If adding tests, create `MmLogView.Tests` (xUnit preferred), mirror source namespaces, and name files as `<ClassName>Tests.cs`.

## Commit & Pull Request Guidelines
- Git history is not available in this workspace snapshot (`.git` missing), so no existing commit convention can be derived.
- Use Conventional Commits (for example `feat: add line jump validation`, `fix: handle empty search text`).
- PRs should include: change summary, rationale, verification steps/commands, and screenshots for UI changes.
- Keep PR scope focused; avoid formatting-only churn across unrelated files.
