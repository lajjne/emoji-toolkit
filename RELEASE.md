# Update with new emoji

1. Download new [`emoji.json`](/emoji.json) from https://github.com/joypixels/emoji-toolkit/blob/master/emoji.json.
2. Generate  `Emoji.generated.cs` with `dotnet run` in the `\src\Generator` folder.
3. Add unit tests for the new emoji version.

# Release checklist

1. Update `VersionPrefix` in `Directory.build.props`.
2. Verify that all unit tests are ok.
2. Commit and push the changes to `main`.
3. [Create a release](https://github.com/lajjne/emoji-toolkit/releases/new) and tag it with the version number.
   This triggers [publish.yml](https://github.com/lajjne/emoji-toolkit/actions/workflows/publish.yml) which publishes a nuget package to https://www.nuget.org/.

