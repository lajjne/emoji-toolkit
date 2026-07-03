# AGENTS.md

Instructions for GitHub Copilot and other AI coding agents working in this repository.

## Scope

These instructions apply to the entire repository.

## Project overview

- Language/runtime: C# on .NET 10 (`net10.0`)
- Main library: `src\EmojiToolkit`
- Tests: `test\EmojiToolkit.Tests` (MSTest)
- Data source: `emojibase-data` (npm package)
- Code generator: `src\Generator`

## Build, test, and packaging

Use these commands from the repository root:

```powershell
dotnet build .\src\EmojiToolkit\EmojiToolkit.csproj
dotnet test .\test\EmojiToolkit.Tests\EmojiToolkit.Tests.csproj
dotnet pack -c Release .\src\EmojiToolkit\EmojiToolkit.csproj
```

## Generated code and data updates

- `src\EmojiToolkit\Emoji.generated.cs` is generated output.
- Do not hand-edit `Emoji.generated.cs` unless absolutely necessary.
- If `emojibase-data` changes, regenerate:

```powershell
dotnet run --project .\src\Generator\Generator.csproj
```

- Commit related changes together (`package.json`/`package-lock.json`, generated code, and tests/docs as needed).

### Updating with new emoji

When updating this repo to a newer emoji dataset, follow this workflow:

1. Install/update `emojibase-data` in the repository root:

```powershell
npm install
# or pin a specific version:
npm install emojibase-data@<unicode-version>
```

2. Regenerate `src\EmojiToolkit\Emoji.generated.cs`:

```powershell
dotnet run --project .\src\Generator\Generator.csproj
```

3. Add or update unit tests for the new emoji version in `test\EmojiToolkit.Tests\EmojiTest.cs`.
4. Run tests:

```powershell
dotnet test .\test\EmojiToolkit.Tests\EmojiToolkit.Tests.csproj
```

5. Commit related updates together (`package.json`/`package-lock.json`, `src\EmojiToolkit\Emoji.generated.cs`, and test changes).

## Editing guidelines

- Follow `.editorconfig` conventions (file-scoped namespaces, braces, `var` preferences, spacing/newlines).
- Keep changes focused and minimal; avoid unrelated refactors.
- Preserve public API behavior unless the task explicitly requires a breaking change.
- Prefer updating existing patterns in `Emoji.cs` and `EmojiTest.cs` instead of introducing new styles.

## Testing expectations

- Add or update tests for behavior changes in `test\EmojiToolkit.Tests\EmojiTest.cs`.
- Ensure existing tests continue to pass before finishing.

## Files and folders to avoid touching unless needed

- `bin\`, `obj\`, and `TestResults\` outputs
- Workflow files under `.github\workflows\` unless the task is CI/release-related

## Pull request expectations for agents

- Keep commit messages and PR descriptions explicit about:
  - what changed
  - why it changed
  - whether generated files were regenerated
