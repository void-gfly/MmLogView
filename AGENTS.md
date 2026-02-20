# AGENTS.md

This file defines practical guidance for coding agents working in `E:\dot_net\MmLogView`.
Follow repository conventions first, then this document.

## 1) Repository Overview

- Project type: single WPF desktop app on `.NET 10` (`net10.0-windows`).
- Solution/project: `MmLogView.sln`, `MmLogView.csproj`.
- Entry points: `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`.
- Core logic: `Core/` (mapped file read/index, search, recent files, markdown export).
- App state/behavior: `ViewModels/` (primarily `MainViewModel`).
- Custom controls: `Controls/` (`LogViewport`, `JsonViewport`).
- UI helpers: `Converters/`, `Themes/`.
- Localization: `Properties/Resources*.resx` + `QS.WPF.Toolkit.GlobalizationSourceGenerator`.
- CI release workflow: `.github/workflows/release.yml`.

## 2) Build / Lint / Test Commands

### Build & run

- Restore: `dotnet restore MmLogView.sln`
- Build (Debug): `dotnet build MmLogView.sln`
- Build (Release): `dotnet build MmLogView.csproj -c Release`
- Run app: `dotnet run --project MmLogView.csproj`
- Clean: `dotnet clean MmLogView.sln`

### Publish

- Publish self-contained single-file `win-x64`:
  `dotnet publish MmLogView.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish`

### Lint / formatting

- No dedicated linter (StyleCop/Analyzer config) is committed today.
- Recommended formatting check (when available in SDK):
  `dotnet format MmLogView.sln --verify-no-changes`
- Auto-fix formatting:
  `dotnet format MmLogView.sln`

### Tests

- There is currently **no test project** in this repository.
- If you add tests, create `MmLogView.Tests` (xUnit preferred).

#### Single-test execution (future-proof examples)

- Run all tests once test project exists:
  `dotnet test MmLogView.Tests/MmLogView.Tests.csproj`
- Run one test class:
  `dotnet test MmLogView.Tests/MmLogView.Tests.csproj --filter FullyQualifiedName~MainViewModelTests`
- Run one test method:
  `dotnet test MmLogView.Tests/MmLogView.Tests.csproj --filter FullyQualifiedName~MainViewModelTests.OpenFile_LoadsMarkdown`

### Manual smoke test (required for functional changes)

- Start app with `dotnet run --project MmLogView.csproj`.
- Verify at least one `.log`/`.txt`, one `.md`, and one `.json` path if impacted.
- Validate search, navigation, and mode switching for changed behavior.

## 3) Code Style Guidelines

### Language/runtime conventions

- Keep nullable references enabled (`<Nullable>enable</Nullable>`).
- Keep implicit usings enabled (`<ImplicitUsings>enable</ImplicitUsings>`).
- Prefer file-scoped namespaces (`namespace X;`).
- Use UTF-8 encoding and 4-space indentation.

### Imports

- Keep `using` directives minimal and explicit when helpful for readability.
- Order: BCL/framework usings first, then third-party, then project usings.
- Remove unused usings in touched files.

### Naming

- Types/methods/properties/events: `PascalCase`.
- Private fields: `_camelCase`.
- Local variables/parameters: `camelCase` with intent-revealing names.
- XAML named elements: descriptive `x:Name` (for example `RecentDropdownBtn`).
- Command properties should end with `Command`.

### Formatting & structure

- Prefer concise methods, extract helpers for repeated logic.
- Keep one responsibility per method where practical.
- Keep mode-specific logic coherent:
  - Log mode: `LogViewport`
  - Markdown mode: `MarkdownScrollViewer`
  - JSON mode: `JsonViewport`

### Types & collections

- Prefer explicit domain types over `object`.
- Use nullable annotations intentionally (`?`, null checks, guard clauses).
- Use `ObservableCollection<T>` for bindable collections.
- Prefer immutable locals (`var` + no reassignment) when clear.

### MVVM boundaries

- Put UI behavior/state transitions in ViewModels whenever possible.
- Keep file/index/search logic in `Core/`; do not duplicate it in UI layers.
- Use commands (`RelayCommand`) instead of code-behind for feature behavior.
- Code-behind is acceptable for view wiring (drag/drop, context menu placement, etc.).

### Async, threading, and dispatcher

- Use `async/await` for I/O or long-running work.
- Marshal UI updates through WPF dispatcher where needed.
- Avoid blocking UI thread during indexing/export operations.

### Error handling

- Never swallow exceptions silently.
- Do not use empty `catch` blocks.
- Catch only where user-visible recovery/reporting is needed.
- Include actionable messages in status bar/dialogs.
- Do not introduce speculative fallback branches that mask real failures.

## 4) Localization Rules (Strict)

- For any new UI text key, update **both**:
  - `Properties/Resources.resx`
  - `Properties/Resources.en-US.resx`
- Ensure `Properties/Resources.Designer.cs` exposes the generated property.
- In XAML, use existing `ResourcesExtension` binding pattern.
- Validate runtime language switch (`zh-CN` / `en-US`) after text changes.

## 5) File and Change Scope Rules

- Keep edits minimal and focused on requested behavior.
- Do not modify generated outputs (`bin/`, `obj/`, `publish/`).
- Avoid broad formatting-only churn in unrelated files.
- Reuse existing patterns in neighboring files before introducing new abstractions.

## 6) Git / PR Expectations

- Prefer concise commit messages (Conventional Commits recommended):
  - `feat: ...`
  - `fix: ...`
  - `refactor: ...`
  - `docs: ...`
- PRs should include:
  - What changed and why
  - Verification commands run
  - UI screenshots for visual changes

## 7) Agent Rule Sources Check

- Checked for Cursor rules:
  - `.cursorrules` -> not found
  - `.cursor/rules/` -> not found
- Checked for Copilot instructions:
  - `.github/copilot-instructions.md` -> not found
- Therefore this `AGENTS.md` is the primary in-repo agent guidance.

## 8) Quick Pre-Completion Checklist

- Build passes: `dotnet build MmLogView.sln`
- Formatting is clean (if used): `dotnet format ... --verify-no-changes`
- Manual smoke test completed for changed features
- Localization keys synced in both `.resx` files (if UI text changed)
- No unrelated files modified
