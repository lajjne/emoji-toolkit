# Release checklist

1. Update `VersionPrefix` in `Directory.build.props`.
2. Commit and push the changes.
3. [Create a release](https://github.com/lajjne/emoji-toolkit/releases/new) and tag it with the version number.
   This triggers [publish.yml](https://github.com/lajjne/emoji-toolkit/actions/workflows/publish.yml) which publishes a nuget package to https://www.nuget.org/.
