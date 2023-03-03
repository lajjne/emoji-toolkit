# Emoji toolkit

[![Build](https://github.com/lajjne/emoji-toolkit/actions/workflows/build.yml/badge.svg)](https://github.com/lajjne/emoji-toolkit/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/EmojiToolkit.svg?label=Nuget)](https://www.nuget.org/packages/EmojiToolkit)

A C# toolkit for working with emoji.

## Usage

The static `Emoji` class has methods for converting emoji into various formats, including conversion to html.

```csharp
// get an emoji by shortcode
Emoji.Get(":smiley:").Raw; // 😃

// get an emoji by ascii equivalent
Emoji.Get(":-D").Raw; // 😃

// get an emoji by raw unicode string
Emoji.Get("😃").Name; // grinning face with big eyes

// get the ascii equivalent of an emoji
Emoji.Ascii("😉"); // ;)

// get <img> tag for the specified emoji
Emoji.Image(":wink:"); // <img class="emoji" alt="😉" title=":wink:" src="/emoji/1f609.png" />

// get the raw unicode equivalent for an emoji shortcode
Emoji.Raw(":smiley:"); // 😃

// get the shortcode for a raw unicode string
Emoji.Shortcode("😃"); // :smiley:

// get <span> tag for the specified emoji
Emoji.Span(":wink:"); // <span class="emoji" title=":wink:">😉</span>

// replace emoji shortcodes and raw unicode strings with their ascii equivalents
Emoji.Asciify("😉 :wink:"); // ;) ;)

// replace emoji shortcodes with raw unicode strings.
Emoji.Emojify("it's raining :cat:s and :dog:s!"); // it's raining 🐱s and 🐶s!

// replace raw unicode strings with emoji shortcodes
Emoji.Demojify("it's raining 🐱s and 🐶s!"); // it's raining :cat:s and :dog:s!

// replace emoji shortcodes and raw unicode strings with <img> tags
Emoji.Imagify("it's raining :cat:s and 🐶s!"); // it's raining <img class="emoji" alt="🐱" title=":cat:" src="/emoji/1f431.png" />s and <img class="emoji" alt="🐶" title=":dog:" src="/emoji/1f436.png" />s! 

// replace emoji shortcodes and raw unicode strings with <span> tags
Emoji.Spanify("it's raining :cat:s and 🐶s!"); // it's raining <span class="emoji" title=":cat:">🐱</span>s and <span class="emoji" title=":dog:">🐶</span>s! 

// find emoji by name, category, shortcodes and tags
Emoji.Find("smile").First().Raw; // 😃

// determine whether a string is comprised solely of emoji
Emoji.IsEmoji("🐱🐶"); // true
Emoji.IsEmoji("it's raining 🐱s and 🐶s!"); // false
```

## Notes

The full list of emoji in `Emoji.generated.cs` was generated from [`emoji.json`](/emoji.json),
which was originally downloaded from https://github.com/joypixels/emoji-toolkit/blob/master/emoji.json.

To re-generate the file execute `dotnet run` from the  `\src\Generator` folder:

```shell
\src\Generator> dotnet run
```
