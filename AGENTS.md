# Repository Guidelines

## Project Structure & Module Organization
- `MmLogView.sln` and `MmLogView.csproj` define a single WPF desktop app targeting `net10.0-windows`.
- UI entry points: `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `GoToLineDialog.cs`.
- Core logic lives in `Core/` (mapped file reading, line index, recent files, Markdown-to-PDF export).
- View behavior and state are in `ViewModels/` (`MainViewModel`, JSON tree/view models, relay commands).
- Custom rendering/interactive controls are in `Controls/` (`LogViewport`, `JsonViewport`), and converters are in `Converters/`.
- Theme dictionaries are in `Themes/`; localization resources are managed through `Properties/*.resx` with `QS.WPF.Toolkit.GlobalizationSourceGenerator`.
- CI release workflow is in `.github/workflows/release.yml`; build outputs are `bin/`, `obj/`, and `publish/` (generated artifacts, do not edit manually).

## Build, Test, and Development Commands
- Restore dependencies: `dotnet restore MmLogView.sln`
- Build (Debug): `dotnet build MmLogView.sln`
- Run locally: `dotnet run --project MmLogView.csproj`
- Release build: `dotnet build MmLogView.csproj -c Release`
- Publish single-file `win-x64` package:  
  `dotnet publish MmLogView.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish`
- Optional cleanup before reproducible builds: `dotnet clean MmLogView.sln`

## Coding Style & Naming Conventions
- C# uses nullable reference types and implicit usings (`<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`).
- Use 4-space indentation; UTF-8 encoding for all source files.
- Naming: `PascalCase` for types/methods/properties, `_camelCase` for private fields, descriptive XAML `x:Name` values (for example `RecentDropdownBtn`).
- Keep UI behavior in ViewModels when possible; avoid duplicating file/line-processing logic already in `Core/`.
- Keep mode-specific behavior coherent: log mode (`LogViewport`), markdown mode (`MarkdownScrollViewer`), and JSON mode (`JsonViewport`).

## Localization Workflow (Important)
- This project uses `QS.WPF.Toolkit.GlobalizationSourceGenerator` + `ResourcesExtension` for WPF bindings. For any new UI text key, update **both** `Properties/Resources.resx` and `Properties/Resources.en-US.resx`.
- Ensure `Properties/Resources.Designer.cs` contains the corresponding strongly-typed property (for example `BtnExportPdf`). If missing, regenerate/sync it before finishing.
- When binding text in XAML, prefer `ResourcesExtension` pattern already used in project (for example `Path=BtnExportPdf`) and verify the key renders in both Chinese and English.
- PR/self-check for new controls must include: resource key added in both `.resx` files, `Resources.Designer.cs` property exists, and runtime language switch shows non-empty text.

## Testing Guidelines
- No dedicated test project is currently present.
- Minimum verification for changes: run `dotnet build` and smoke-test via `dotnet run` (open at least one `.log/.txt`, one `.md`, and one `.json` file when relevant to your changes).
- If adding tests, create `MmLogView.Tests` (xUnit preferred), mirror source namespaces, and name files as `<ClassName>Tests.cs`.

## Commit & Pull Request Guidelines
- Git history is available; existing commits are mostly concise Chinese summaries with occasional Conventional Commit style.
- Preferred commit style: concise, scoped messages (Conventional Commits are recommended, for example `feat: add line jump validation`, `fix: handle empty search text`).
- PRs should include: change summary, rationale, verification steps/commands, and screenshots for UI changes.
- Keep PR scope focused; avoid formatting-only churn across unrelated files.
