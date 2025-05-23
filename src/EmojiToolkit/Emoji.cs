using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmojiToolkit;

/// <summary>
/// Utility class for working with emoji.
/// </summary>
public static partial class Emoji {

    // lookup tables for emoji
    private static readonly Dictionary<string, EmojiRecord> _asciiToEmoji = [];
    private static readonly Dictionary<string, EmojiRecord> _pointToEmoji = [];
    private static readonly Dictionary<string, EmojiRecord> _codeToEmoji = [];

    // default settings for emoji images and spans
    private const string DEFAULT_CLASS = "emoji";
    private const string DEFAULT_PATH = "/emoji/";
    private const string DEFAULT_EXT = ".png";

    // regular expression for matching ascii emoji
    [GeneratedRegex(IGNORE_PATTERN + @"|(?<=\s|^)(" + ASCII_PATTERN + @")(?=\s|$|[!,\.])")]
    private static partial Regex AsciiRegex();

    // regular expression for matching raw unicode emoji
    [GeneratedRegex(IGNORE_PATTERN + "|(" + RAW_PATTERN + ")")]
    private static partial Regex RawRegex();

    // regular expression for matching emoji shortcodes
    [GeneratedRegex(IGNORE_PATTERN + "|(" + SHORT_PATTERN + ")", RegexOptions.IgnoreCase)]
    private static partial Regex ShortRegex();

    /// <summary>
    /// Static constructor for <see cref="Emoji"/>.
    /// </summary>
    static Emoji() {
        // initialize lookup tables
        foreach (var emoji in All) {
            if (emoji.Ascii != null) {
                foreach (var ascii in emoji.Ascii) {
                    _asciiToEmoji.Add(ascii, emoji);
                }
            }

            foreach (var codepoint in emoji.Codepoints) {
                _pointToEmoji.Add(codepoint, emoji);
            }

            foreach (var shortcode in emoji.Shortcodes) {
                _codeToEmoji.Add(shortcode, emoji);
            }
        }
    }

    /// <summary>
    /// Gets the emoji associated with the specified shortcode or raw unicode string.
    /// </summary>
    /// <param name="value">Emoji shortcode, ascii emoji or raw unicode string.</param>
    /// <returns>The <see cref="Emoji"/>, or <c>null</c> if no match was found.</returns>
    public static EmojiRecord Get(string value) {
        ArgumentNullException.ThrowIfNull(value);

        // short code?       
        if (value.StartsWith(':') && _codeToEmoji.TryGetValue(value, out var e1)) {
            return e1;
        }

        // ascii?
        if (_asciiToEmoji.TryGetValue(value, out var e2)) {
            return e2;
        }

        // lookup emoji from codepoint
        // TODO: some kind of sanity check on length before converting to codepoint? 
        var cp = ToCodePoint(value);
        if (_pointToEmoji.TryGetValue(cp, out var e3)) {
            return e3;
        }

        return null;
    }

    /// <summary>
    /// Gets the ascii equivalent of the emoji with the specified shortcode or raw unicode string.
    /// </summary>
    /// <param name="value">Emoji shortcode or raw unicode string.</param>
    /// <returns>The ascii equivalent of the emoji, or <c>null</c>.</returns>
    public static string Ascii(string value) {
        return Get(value)?.Ascii?.FirstOrDefault();
    }

    /// <summary>
    /// Gets &lt;img&gt; tag for the emoji with the specified shortcode or raw unicode string.
    /// </summary>
    /// <param name="value">Emoji shortcode or raw unicode string.</param>
    /// <param name="css">CSS class to apply.</param>
    /// <param name="path">Path (url) to image folder.</param>
    /// <param name="ext">Image file extension.</param>
    /// <returns>An &lt;img&gt; tag for the emoji, or <c>null</c>.</returns>
    public static string Image(string value, string css = null, string path = null, string ext = null) {
        var emoji = Get(value);
        if (emoji != null) {

            return $@"<img class=""{css ?? DEFAULT_CLASS}"" alt=""{emoji.Raw}"" title=""{emoji.Shortcodes[0]}"" src=""{path ?? DEFAULT_PATH}{emoji.Codepoints[0]}{ext ?? DEFAULT_EXT}"" />";
        }
        return null;
    }

    /// <summary>
    /// Gets the raw unicode string of the emoji associated with the shortcode.
    /// </summary>
    /// <param name="shortcode">Emoji shortcode.</param>
    /// <returns>The raw unicode string, or <c>null</c>.</returns>
    public static string Raw(string shortcode) {
        return Get(shortcode)?.Raw;
    }

    /// <summary>
    /// Gets the shortcode of the emoji represented by the raw unicode string.
    /// </summary>
    /// <param name="raw">The raw unicode string of the emoji.</param>
    /// <returns>The shortcode referring to the emoji, or <c>null</c> if no match was found.</returns>
    public static string Shortcode(string raw) {
        return Get(raw)?.Shortcodes?.FirstOrDefault();
    }

    /// <summary>
    /// Gets &lt;span&gt; tag for the emoji with the specified shortcode or raw unicode string.
    /// </summary>
    /// <param name="value">Emoji shortcode or raw unicode string.</param>
    /// <param name="css">CSS class to apply.</param>
    /// <returns>A &lt;span&gt; tag for the emoji, or <c>null</c>.</returns>
    public static string Span(string value, string css = null) {
        var emoji = Get(value);
        if (emoji != null) {
            return $@"<span class=""{css ?? DEFAULT_CLASS}"" title=""{emoji.Shortcodes[0]}"">{emoji.Raw}</span>";
        }
        return null;
    }

    /// <summary>
    /// Replaces emoji shortcodes and raw unicode strings in <paramref name = "text" /> with their ascii equivalent, e.g. :wink: -> ;). 
    /// This is useful for systems that don't support unicode or images.
    /// </summary>
    /// <param name="text">The text to asciify.</param>
    /// <returns>A string with ascii replacements.</returns>
    public static string Asciify(string text) {
        if (text != null) {
            // first pass replaces shortcodes
            text = ShortRegex().Replace(text, match => {
                // return raw unicode (or the entire match if we couldn't find a matching emoji)
                return Ascii(match.Groups[1].Value) ?? match.Value;
            });

            // second pass replaces raw unicode 
            text = RawRegex().Replace(text, match => {
                // return ascii (or the entire match if we couldn't find a matching emoji)
                return Ascii(match.Groups[1].Value) ?? match.Value;
            });
        }
        return text;
    }

    /// <summary>
    /// Replaces emoji shortcodes in <paramref name="text"/> with raw unicode strings.
    /// </summary>
    /// <param name="text">The text to emojify.</param>
    /// <param name="ascii"><c>true</c> to also replace ascii emoji, otherwise <c>false</c>.</param>
    /// <returns>A string with emoji represented as raw unicode strings.</returns>
    public static string Emojify(string text, bool ascii = false) {
        if (text != null) {
            // first pass replaces shortcodes
            text = ShortRegex().Replace(text, match => {
                // return raw unicode (or the entire match if we couldn't find a matching emoji)
                return Raw(match.Groups[1].Value) ?? match.Value;
            });

            // second pass replaces ascii 
            if (ascii) {
                text = AsciiRegex().Replace(text, match => {
                    // check if the emoji exists in our dictionary
                    var ascii = match.Groups[1].Value;
                    if (_asciiToEmoji.TryGetValue(ascii, out var emoji)) {
                        return emoji.Raw;
                    }
                    // we didn't find a replacement so just return the entire match
                    return match.Value;
                });
            }
        }
        return text;
    }

    /// <summary>
    /// Replaces raw unicode string in <paramref name="text"/> with emoji shortcodes.
    /// </summary>
    /// <param name="text">The text to demojify.</param>
    /// <returns>A string with emoji represented as emoji shortcodes.</returns>
    public static string Demojify(string text) {
        if (text != null) {
            text = RawRegex().Replace(text, match => {
                // return shortcode (or the entire match if we couldn't find a matching emoji)
                return Shortcode(match.Groups[1].Value) ?? match.Value;
            });
        }
        return text;
    }

    /// <summary>
    /// Replaces emoji shortcodes and raw unicode strings in <paramref name="text"/> with &lt;img&gt; tags.
    /// </summary>
    /// <param name="text">The text to imagify.</param>
    /// <param name="ascii"><c>true</c> to also replace ascii emoji, otherwise <c>false</c>.</param>
    /// <param name="css">CSS class to apply.</param>
    /// <param name="path">Path (url) to image folder.</param>
    /// <param name="ext">Image file extension.</param>
    /// <returns>A string with emoji represented as &lt;img&gt; tags.</returns>    
    public static string Imagify(string text, bool ascii = false, string css = null, string path = null, string ext = null) {

        if (text != null) {
            // first pass replaces shortcodes with raw unicode strings
            text = Emojify(text, ascii);

            // second pass replaces raw unicode strings with <img> tags
            text = RawRegex().Replace(text, match => {
                // return image tag (or the entire match if we couldn't find a matching emoji)
                return Image(match.Groups[1].Value, css, path, ext) ?? match.Value;
            });
        }
        return text;
    }

    /// <summary>
    /// Replaces emoji shortcodes and raw unicode strings in <paramref name="text"/> with &lt;span&gt; tags.
    /// </summary>
    /// <param name="text">The text to spanify.</param>
    /// <param name="ascii"><c>true</c> to also replace ascii emoji, otherwise <c>false</c>.</param>
    /// <param name="css">CSS class to apply.</param>
    /// <returns>A string with emoji represented as &lt;span&gt; tags.</returns>    
    public static string Spanify(string text, bool ascii = false, string css = null) {

        if (text != null) {
            // first pass replaces shortcodes with raw unicode strings
            text = Emojify(text, ascii);

            // second pass replaces raw unicode strings with <span> tags
            text = RawRegex().Replace(text, match => {
                // return span tag (or the entire match if we couldn't find a matching emoji)
                return Span(match.Groups[1].Value, css) ?? match.Value;
            });
        }
        return text;
    }

    /// <summary>
    /// Returns emoji with matching name, category, shortcodes or tags.
    /// </summary>
    /// <param name="q">The value to search for.</param>
    /// <returns>A list of emoji.</returns>
    public static IEnumerable<EmojiRecord> Find(string q) {
        ArgumentNullException.ThrowIfNull(q);
        return All.Where(emoji => emoji.Name.Contains(q) || emoji.Category.Contains(q) || (emoji.Shortcodes?.Any(x => x.Contains(q)) ?? false) || (emoji.Tags?.Any(x => x.Contains(q)) ?? false));
    }

    /// <summary>
    /// Determines whether a string is comprised solely of emoji, optionally with a maximum number of symbols.
    /// Can be used to determine whether a message consists of more than <paramref name="maxSymbolCount"/> emoji for purposes such as displaying at a larger size.
    /// </summary>
    /// <param name="text">The text to investigate.</param>
    /// <param name="maxSymbolCount">Maximum number of allowed symbols.</param>
    /// <returns></returns>
    public static bool IsEmoji(string text, int maxSymbolCount = int.MaxValue) {
        var count = 0;
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext()) {
            var raw = enumerator.GetTextElement();
            if (raw == "\n" || raw == "\r" || raw == "\t" || raw == " ") {
                // whitespace doesn't count
                continue;
            }

            if (++count > maxSymbolCount) {
                return false;
            }
            
            if (Get(raw) == null) {
                // a non-emoji symbol was found
                return false;
            }
        }
        return count > 0 && count <= maxSymbolCount;
    }

    /// <summary>
    /// Converts a hex code point/code pair(s) to a raw unicode string.
    /// </summary>
    /// <param name="codepoint">Hex encoded code point or code pair(s).</param>
    /// <returns>A unicode string.</returns>
    public static string FromCodePoint(string codepoint) {
        var bytes = EnumerateBytes(codepoint).ToArray();
        return Encoding.Unicode.GetString(bytes);
    }

    /// <summary>
    /// Convert a raw unicode string to its hex code point/code pair(s).
    /// </summary>
    /// <param name="raw">A unicode string.</param>
    /// <returns></returns>
    internal static string ToCodePoint(string raw) {
        var codepoint = "";
        for (var i = 0; i < raw.Length; i += char.IsSurrogatePair(raw, i) ? 2 : 1) {
            if (i > 0) {
                codepoint += "-";
            }
            codepoint += string.Format("{0:x4}", char.ConvertToUtf32(raw, i));
        }
        return codepoint.ToLower();
    }

    /// <summary>
    /// Converts a hex code point/code pair(s) to UTF16 surrogate pair(s).
    /// </summary>
    /// <param name="codepoint">Hex encoded code point or code pair(s).</param>
    /// <returns></returns>
    internal static string ToSurrogate(string codepoint) {
        var raw = FromCodePoint(codepoint);
        var s2 = "";
        for (var x = 0; x < raw.Length; x++) {
            s2 += string.Format("\\u{0:x4}", (int)raw[x]);
        }
        return s2;
    }

    /// <summary>
    /// Returns an enumerator that iterates over the little endian bytes in the UTF16 encoding of the specified codepoint.
    /// </summary>
    /// <param name="codepoint"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static IEnumerable<byte> EnumerateBytes(string codepoint) {
        var codepoints = Array.ConvertAll(codepoint.Split('-'), (i) => uint.Parse(i, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat));
        foreach (var cp in codepoints) {
            foreach (var b in EnumerateBytes(cp)) {
                yield return b;
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates over the little endian bytes in the UTF16 encoding of the specified codepoint.
    /// </summary>
    /// <param name="codepoint"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static IEnumerable<byte> EnumerateBytes(uint codepoint) {
        if (codepoint <= 0xFFFF) {
            // U+0000 to U+D7FF and U+E000 to U+FFFF
            yield return (byte)(codepoint);
            yield return (byte)(codepoint >> 8);
        } else if (codepoint >= 0x10000 && codepoint <= 0x10FFFF) {
            // U+10000 to U+10FFFF
            var newVal = codepoint - 0x010000; // leaving 20 bits
            var high = (ushort)((newVal >> 10) + 0xD800);
            yield return (byte)(high);
            yield return (byte)(high >> 8);

            var low = (ushort)((newVal & 0x03FF) + 0xDC00);
            yield return (byte)(low);
            yield return (byte)(low >> 8);
        } else {
            throw new Exception("Unsupported code point: " + codepoint);
        }
    }
}

/// <summary>
/// Represents an emoji.
/// </summary>
/// <param name="Raw">Raw unicode <c>string</c> of the emoji.</param>
/// <param name="Name">The emoji name according to http://unicode.org/Public/emoji/15.0/emoji-test.txt.</param>
/// <param name="Category">Emoji category.</param>
/// <param name="Codepoints">Code points according to http://unicode.org/Public/emoji/15.0/emoji-test.txt.</param>
/// <param name="Shortcodes">A list of names (colon-encapsulated, snake_cased) uniquely referring to the emoji.</param>
/// <param name="Ascii">Ascii representations of the emoji.</param>
/// <param name="Tags">A list of tags/keywords associated with the emoji. Multiple emoji can share the same tags.</param>
/// <param name="Version">Unicode version when the emoji was added.</param>
public record EmojiRecord(string Raw, string Name, string Category, string[] Codepoints, string[] Shortcodes, string[] Ascii, string[] Tags, double Version);
