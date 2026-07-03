# Update with new emoji

1. Install/update `emojibase-data` with `npm install emojibase-data@<version>`
2. Generate `Emoji.generated.cs` with `dotnet run --project .\src\Generator\Generator.csproj`
3. Add unit tests for the new emoji version.

# Release checklist

1. Update `VersionPrefix` in `Directory.build.props`.
2. Verify that all unit tests are ok.
2. Commit and push the changes to `main`.
3. [Create a release](https://github.com/lajjne/emoji-toolkit/releases/new) and tag it with the version number.
   This triggers [publish.yml](https://github.com/lajjne/emoji-toolkit/actions/workflows/publish.yml) which publishes a nuget package to https://www.nuget.org/.
