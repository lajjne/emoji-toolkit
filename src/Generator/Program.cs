using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Generator;

/// <summary>
/// Generates Emoji.generated.cs from emoji.json
/// </summary>
public partial class Program {

    [GeneratedRegex(@"uc\d+")]
    private static partial Regex VersionTagRegex();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    static int Main(string[] args) {
        try {
            // parse emoji.json into dictionary codepoint -> emoji
            var path = Path.GetFullPath(args.Length == 1 ? args[0] : "../../emoji.json");
            Console.WriteLine("Loading " + path);

            var json = File.ReadAllText(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, Emoji>>(json);

            // remove ascii symbols and digits
            var chars = "0123456789#*";
            foreach (var c in chars) {
                var codepoint = ToCodePoint(c.ToString());
                dict.Remove(codepoint);
            }

            // write regex patterns and dictionaries to partial class with UTF-8 encoding
            path = Path.GetFullPath(Path.Combine("../EmojiToolkit", "Emoji.generated.cs"));
            Console.WriteLine("Writing " + path);
            using (var sw = new StreamWriter(path, false, new UTF8Encoding(false))) {
                sw.WriteLine(@"namespace EmojiToolkit;");
                sw.WriteLine();
                sw.WriteLine(@"public static partial class Emoji {");
                sw.WriteLine();

                var asciis = dict.Values.Where(x => x.Ascii.Length != 0);
                sw.WriteLine("""
                        /// <summary>
                        /// Regular expression pattern to match ascii emoji.
                        /// </summary>
                    """);
                sw.Write(@"    private const string ASCII_PATTERN = @""");
                for (var i = 0; i < asciis.Count(); i++) {
                    var emoji = asciis.ElementAt(i);
                    for (var j = 0; j < emoji.Ascii.Length; j++) {
                        sw.Write(Regex.Escape(emoji.Ascii[j]));
                        if (j < emoji.Ascii.Length - 1) {
                            sw.Write("|");
                        }
                    }
                    if (i < asciis.Count() - 1) {
                        sw.Write("|");
                    }
                }
                sw.WriteLine(@""";");
                sw.WriteLine();

                sw.WriteLine("""
                        /// <summary>
                        /// Regular expression pattern where we should ignore emoji.
                        /// </summary>
                    """);
                sw.WriteLine(@"    private const string IGNORE_PATTERN = @""<object[^>]*>.*?</object>|<span[^>]*>.*?</span>|<(?:object|embed|svg|img|div|span|p|a)[^>]*>"";");
                sw.WriteLine();

                sw.WriteLine("""
                        /// <summary>
                        /// Regular expression pattern to match raw unicode emoji.
                        /// </summary>
                    """);
                sw.Write(@"    private const string RAW_PATTERN = @""");
                // NOTE: these must be ordered by length of the unicode code point
                var codepoints = dict.Values.SelectMany(e => e.CodePoints.BaseAndFullyQualified).OrderByDescending(cp => cp.Length).ToList();
                for (var i = 0; i < codepoints.Count; i++) {
                    var cp = codepoints.ElementAt(i);
                    sw.Write(ToSurrogateString(cp));
                    if (i < codepoints.Count - 1) {
                        sw.Write("|");
                    }
                }
                sw.WriteLine(@""";");
                sw.WriteLine();

                sw.WriteLine("""
                        /// <summary>
                        /// Regular expression pattern to match emoji shortcodes.
                        /// </summary>
                    """);
                sw.Write(@"    private const string SHORT_PATTERN = @""");
                for (var i = 0; i < dict.Count; i++) {
                    var emoji = dict.ElementAt(i).Value;
                    if (i > 0) {
                        sw.Write("|");
                    }
                    sw.Write(Regex.Escape(emoji.Shortname));
                    for (var j = 0; j < emoji.ShortnameAlternates.Length; j++) {
                        sw.Write("|");
                        sw.Write(Regex.Escape(emoji.ShortnameAlternates[j]));
                    }
                }
                sw.WriteLine(@""";");
                sw.WriteLine();

                sw.WriteLine("""
                        /// <summary>
                        /// A list of all emoji.
                        /// </summary>
                    """);
                sw.WriteLine(@"    public static readonly EmojiRecord[] All = new EmojiRecord[] {");

                for (var i = 0; i < dict.Count; i++) {
                    var emoji = dict.ElementAt(i).Value;

                    // new ("😃", "grinning face with big eyes", "people", new[] { "1f603" }, new[] { ":smiley:" }, new[] { ":-D", "=D" }, new[] { "face", "mouth", "open", "smile", "uc6" });

                    // raw, name, category
                    sw.Write($@"        new (""{FromCodePoint(emoji.CodePoints.FullyQualified)}"", ""{emoji.Name}"", ""{emoji.Category}"", ");

                    // codepoints
                    sw.Write($@"new [] {{ ""{emoji.CodePoints.Base}""");
                    if (emoji.CodePoints.FullyQualified != emoji.CodePoints.Base) {
                        sw.Write($@", ""{emoji.CodePoints.FullyQualified}""");
                    }
                    sw.Write(" }, ");

                    // shortnames
                    sw.Write($@"new [] {{ ""{emoji.Shortname}""");
                    foreach (var alt in emoji.ShortnameAlternates) {
                        sw.Write($@", ""{alt}""");
                    }
                    sw.Write(" }, ");

                    // ascii
                    if (emoji.Ascii.Length != 0) {
                        sw.Write($@"new [] {{ ");
                        for (var j = 0; j < emoji.Ascii.Length; j++) {
                            var ascii = emoji.Ascii[j].Replace("\"", "\\\"").Replace("\\", "\\\\");
                            if (j > 0) {
                                sw.Write(", ");
                            }
                            sw.Write(@$"""{ascii}""");
                        }
                        sw.Write(" }, ");
                    } else {
                        sw.Write("null, ");
                    }

                    // tags
                    var tags = emoji.Keywords.Where(t => !VersionTagRegex().IsMatch(t)).ToArray();

                    if (tags.Length != 0) {
                        sw.Write($@"new [] {{ ");
                        for (var j = 0; j < tags.Length; j++) {
                            var tag = tags[j];
                            if (j > 0) {
                                sw.Write(", ");
                            }
                            sw.Write(@$"""{tag}""");
                        }
                        sw.Write(" }");
                    } else {
                        sw.Write("null");
                    }

                    // version
                    sw.Write($@", {emoji.Version.ToString("G", CultureInfo.InvariantCulture)})");

                    if (i < dict.Count - 1) {
                        sw.WriteLine(",");
                    }
                }
                sw.WriteLine();
                sw.WriteLine(@"    };");

                sw.WriteLine();

                sw.WriteLine(@"}");
            }            
        } catch (Exception e) {
            Console.WriteLine(e.Message);
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Convert a unicode character to its code point/code pair(s)
    /// </summary>
    /// <param name="unicode"></param>
    /// <returns></returns>
    private static string ToCodePoint(string unicode) {
        var codepoint = "";
        for (var i = 0; i < unicode.Length; i += char.IsSurrogatePair(unicode, i) ? 2 : 1) {
            if (i > 0) {
                codepoint += "-";
            }
            codepoint += string.Format("{0:x4}", char.ConvertToUtf32(unicode, i));
        }
        return codepoint.ToLower();
    }

    /// <summary>
    /// Converts a codepoint to unicode surrogate pairs
    /// </summary>
    /// <param name="unicode"></param>
    /// <returns></returns>
    private static string ToSurrogateString(string codepoint) {
        var unicode = FromCodePoint(codepoint);

        var s2 = "";
        for (var x = 0; x < unicode.Length; x++) {
            s2 += string.Format("\\u{0:x4}", (int)unicode[x]);
        }
        return s2;
    }

    /// <summary>
    /// Converts unicode code point/code pair(s) to a unicode character.
    /// </summary>
    /// <param name="codepoint"></param>
    /// <returns></returns>
    internal static string FromCodePoint(string codepoint) {
        var bytes = AsUtf16Bytes(codepoint).ToArray();
        return Encoding.Unicode.GetString(bytes);
    }

    // Little Endian byte order
    public static IEnumerable<byte> AsUtf16Bytes(string codepoint) {
        var _codepoints = codepoint.Split('-').Select(x => uint.Parse(x, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat));
        var bytes = new byte[4];
        foreach (var cp in _codepoints) {
            var count = AsUtf16Bytes(cp, bytes);
            for (var i = 0; i < count; ++i) {
                yield return bytes[i];
            }
        }

    }

    /// <summary>
    /// Returns an iterator that will enumerate over the little endian bytes in the UTF16 encoding of this codepoint.
    /// </summary>
    public static int AsUtf16Bytes(uint codepoint, Span<byte> dest) {
        // U+0000 to U+D7FF and U+E000 to U+FFFF
        if (codepoint <= 0xFFFF) {
            dest[0] = (byte)(codepoint);
            dest[1] = (byte)(codepoint >> 8);
            return 2;
        }

        // U+10000 to U+10FFFF
        if (codepoint >= 0x10000 && codepoint <= 0x10FFFF) {
            var newVal = codepoint - 0x010000; // leaving 20 bits
            var high = (ushort)((newVal >> 10) + 0xD800);
            //System.Diagnostics.Debug.Assert(high <= 0xDBFF && high >= 0xD800);
            dest[0] = (byte)(high);
            dest[1] = (byte)(high >> 8);

            var low = (ushort)((newVal & 0x03FF) + 0xDC00);
            //System.Diagnostics.Debug.Assert(low <= 0xDFFF && low >= 0xDC00);
            dest[2] = (byte)(low);
            dest[3] = (byte)(low >> 8);
            return 4;
        }

        throw new Exception("Unsupported code point: " + codepoint);
    }

}

record Emoji {

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("shortname")]
    public string Shortname { get; set; }

    [JsonPropertyName("shortname_alternates")]
    public string[] ShortnameAlternates { get; set; } = [];

    [JsonPropertyName("ascii")]
    public string[] Ascii { get; set; } = [];

    [JsonPropertyName("code_points")]
    public CodePoints CodePoints { get; set; }

    [JsonPropertyName("keywords")]
    public string[] Keywords { get; set; } = [];

    [JsonPropertyName("unicode_version")]
    public double Version { get; set; }
}

record CodePoints {

    /// <summary>
    /// Full unicode code point minus VS16 and ZWJ.
    /// </summary>
    [JsonPropertyName("base")]
    public string Base { get; set; }

    /// <summary>
    /// Fully qualified code point according to http://unicode.org/Public/emoji/11.0/emoji-test.txt.
    /// </summary>
    [JsonPropertyName("fully_qualified")]
    public string FullyQualified { get; set; }

    /// <summary>
    /// Combination of <see cref="Base"/> and <see cref="FullyQualified"/> codepoints used to identify native unicode.
    /// </summary>
    public string[] BaseAndFullyQualified => Base == FullyQualified ? [Base] : [Base, FullyQualified];

}
