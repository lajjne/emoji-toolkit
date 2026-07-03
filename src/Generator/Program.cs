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
/// Generates Emoji.generated.cs from emojibase-data.
/// </summary>
public class Program {

    private static readonly HashSet<string> ExcludedBaseCodepoints = [
        ToCodePoint("#"),
        ToCodePoint("*"),
        ToCodePoint("0"),
        ToCodePoint("1"),
        ToCodePoint("2"),
        ToCodePoint("3"),
        ToCodePoint("4"),
        ToCodePoint("5"),
        ToCodePoint("6"),
        ToCodePoint("7"),
        ToCodePoint("8"),
        ToCodePoint("9"),
    ];

    private static readonly Dictionary<string, string> CategoryByGroupKey = new(StringComparer.OrdinalIgnoreCase) {
        ["smileys-emotion"] = "smileys",
        ["people-body"] = "people",
        ["animals-nature"] = "nature",
        ["food-drink"] = "food",
        ["travel-places"] = "travel",
        ["activities"] = "activities",
        ["objects"] = "objects",
        ["symbols"] = "symbols",
        ["flags"] = "flags",
    };

    static int Main(string[] args) {
        try {
            var repoRoot = FindRepositoryRoot();
            var emojibaseRoot = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine(repoRoot, "node_modules", "emojibase-data"));
            var locale = args.Length > 1 ? args[1] : "en";

            var dataPath = Path.Combine(emojibaseRoot, locale, "data.json");
            var messagesPath = Path.Combine(emojibaseRoot, locale, "messages.json");
            var groupsPath = Path.Combine(emojibaseRoot, "meta", "groups.json");
            var shortcodesPath = Path.Combine(emojibaseRoot, locale, "shortcodes", "emojibase.json");

            EnsureFileExists(dataPath);
            EnsureFileExists(messagesPath);
            EnsureFileExists(groupsPath);
            EnsureFileExists(shortcodesPath);

            Console.WriteLine("Loading " + dataPath);
            Console.WriteLine("Loading " + messagesPath);
            Console.WriteLine("Loading " + groupsPath);
            Console.WriteLine("Loading " + shortcodesPath);

            var entries = JsonSerializer.Deserialize<List<EmojibaseEntry>>(File.ReadAllText(dataPath)) ?? throw new InvalidDataException("Unable to parse emojibase data.");

            // Parse to ensure the locale messages file is available and valid.
            _ = JsonSerializer.Deserialize<EmojiMessages>(File.ReadAllText(messagesPath)) ?? throw new InvalidDataException("Unable to parse emojibase locale messages.");

            var groups = LoadIndexMap(groupsPath, "groups");
            var subgroups = LoadIndexMap(groupsPath, "subgroups");
            var shortcodes = LoadShortcodes(shortcodesPath);

            var emojis = BuildEmojis(entries, groups, subgroups, shortcodes)
                .Where(e => !ExcludedBaseCodepoints.Contains(e.CodePoints.Base))
                .OrderBy(e => e.Order ?? int.MaxValue)
                .ThenBy(e => e.Index)
                .ToList();

            var outputPath = Path.Combine(repoRoot, "src", "EmojiToolkit", "Emoji.generated.cs");
            Console.WriteLine("Writing " + outputPath);
            WriteOutput(outputPath, emojis);
        } catch (Exception e) {
            Console.WriteLine(e.Message);
            return 1;
        }
        return 0;
    }

    private static List<GeneratedEmoji> BuildEmojis(List<EmojibaseEntry> entries, Dictionary<int, string> groups, Dictionary<int, string> subgroups, Dictionary<string, string[]> shortcodes) {

        var result = new List<GeneratedEmoji>();
        var index = 0;

        foreach (var entry in entries) {
            var entryTags = BuildTags(entry.Tags, entry.Label);
            result.Add(ToGeneratedEmoji(
                entry.Label,
                entry.Hexcode,
                entry.Type,
                entry.Group,
                entry.Subgroup,
                entryTags,
                ParseStringOrArray(entry.Emoticon),
                entry.Version,
                entry.Order,
                index++,
                groups,
                subgroups,
                shortcodes));

            foreach (var skin in entry.Skins) {
                var skinTags = skin.Tags.Length != 0
                    ? BuildTags(skin.Tags, skin.Label)
                    : BuildTags(entry.Tags, skin.Label);

                result.Add(ToGeneratedEmoji(
                    skin.Label,
                    skin.Hexcode,
                    skin.Type,
                    skin.Group ?? entry.Group,
                    skin.Subgroup ?? entry.Subgroup,
                    skinTags,
                    ParseStringOrArray(skin.Emoticon),
                    skin.Version,
                    skin.Order,
                    index++,
                    groups,
                    subgroups,
                    shortcodes));
            }
        }

        return result;
    }

    private static GeneratedEmoji ToGeneratedEmoji(
        string label,
        string sourceHexcode,
        int type,
        int? group,
        int? subgroup,
        string[] tags,
        string[] ascii,
        double version,
        int? order,
        int index,
        Dictionary<int, string> groups,
        Dictionary<int, string> subgroups,
        Dictionary<string, string[]> shortcodes) {

        var normalizedHexcode = NormalizeHexcode(sourceHexcode);
        var fullyQualified = BuildFullyQualifiedCodepoint(normalizedHexcode, type);
        var baseCodepoint = BuildBaseCodepoint(fullyQualified);
        var category = ResolveCategory(normalizedHexcode, group, subgroup, groups, subgroups);

        if (!shortcodes.TryGetValue(normalizedHexcode, out var shortcodeValues) || shortcodeValues.Length == 0) {
            throw new InvalidDataException($"No Emojibase shortcodes found for {sourceHexcode} ({label}).");
        }

        var wrappedShortcodes = shortcodeValues
            .Select(s => ":" + s + ":")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new GeneratedEmoji(
            Name: label,
            Category: category,
            CodePoints: new CodePoints(Base: baseCodepoint, FullyQualified: fullyQualified),
            Shortcodes: wrappedShortcodes,
            Ascii: ascii.Length == 0 ? null : ascii,
            Tags: tags.Length == 0 ? null : tags,
            Version: version,
            Order: order,
            Index: index);
    }

    private static string ResolveCategory(string normalizedHexcode, int? group, int? subgroup, Dictionary<int, string> groups, Dictionary<int, string> subgroups) {

        if (group == null) {
            if (IsRegionalIndicator(normalizedHexcode)) {
                return "regional";
            }
            return "symbols";
        }

        if (!groups.TryGetValue(group.Value, out var groupKey)) {
            return "symbols";
        }

        if (groupKey.Equals("component", StringComparison.OrdinalIgnoreCase)) {
            if (subgroup != null
                && subgroups.TryGetValue(subgroup.Value, out var subgroupKey)
                && subgroupKey.Equals("hair-style", StringComparison.OrdinalIgnoreCase)) {
                return "people";
            }
            return "modifier";
        }

        if (CategoryByGroupKey.TryGetValue(groupKey, out var category)) {
            return category;
        }

        return "symbols";
    }

    private static Dictionary<int, string> LoadIndexMap(string path, string propertyName) {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;

        if (!root.TryGetProperty(propertyName, out var mapElement) || mapElement.ValueKind != JsonValueKind.Object) {
            throw new InvalidDataException($"Unable to find '{propertyName}' in {path}.");
        }

        var map = new Dictionary<int, string>();
        foreach (var item in mapElement.EnumerateObject()) {
            if (int.TryParse(item.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var key)
                && item.Value.ValueKind == JsonValueKind.String) {
                map[key] = item.Value.GetString()!;
            }
        }
        return map;
    }

    private static Dictionary<string, string[]> LoadShortcodes(string path) {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (doc.RootElement.ValueKind != JsonValueKind.Object) {
            throw new InvalidDataException($"Invalid shortcode data in {path}.");
        }

        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in doc.RootElement.EnumerateObject()) {
            var values = ParseStringOrArray(property.Value)
                .Select(x => x.Trim())
                .Where(x => x.Length != 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (values.Length != 0) {
                result[NormalizeHexcode(property.Name)] = values;
            }
        }
        return result;
    }

    private static string[] BuildTags(string[] sourceTags, string label) {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in sourceTags) {
            if (!string.IsNullOrWhiteSpace(tag) && seen.Add(tag)) {
                result.Add(tag);
            }
        }

        foreach (var token in TokenizeLabel(label)) {
            if (seen.Add(token)) {
                result.Add(token);
            }
        }

        return result.ToArray();
    }

    private static IEnumerable<string> TokenizeLabel(string label) {
        if (string.IsNullOrWhiteSpace(label)) {
            yield break;
        }

        var sb = new StringBuilder();
        foreach (var c in label.ToLowerInvariant()) {
            if (char.IsLetterOrDigit(c) || c == '+' || c == '-') {
                sb.Append(c);
                continue;
            }

            if (sb.Length != 0) {
                yield return sb.ToString();
                sb.Clear();
            }
        }

        if (sb.Length != 0) {
            yield return sb.ToString();
        }
    }

    private static string[] ParseStringOrArray(JsonElement value) {
        if (value.ValueKind == JsonValueKind.String) {
            var v = value.GetString();
            return string.IsNullOrWhiteSpace(v) ? [] : [v];
        }

        if (value.ValueKind == JsonValueKind.Array) {
            return value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToArray();
        }

        return [];
    }

    /// <summary>
    /// Write regex patterns and emoji list to partial class with UTF-8 encoding
    /// </summary>
    /// <param name="path"></param>
    /// <param name="emojis"></param>
    private static void WriteOutput(string path, List<GeneratedEmoji> emojis) {
        using var sw = new StreamWriter(path, false, new UTF8Encoding(false));
        sw.WriteLine(@"namespace EmojiToolkit;");
        sw.WriteLine();
        sw.WriteLine(@"public static partial class Emoji {");
        sw.WriteLine();

        var asciiValues = emojis.Where(x => x.Ascii != null).SelectMany(x => x.Ascii!).ToList();
        sw.WriteLine("""
                /// <summary>
                /// Regular expression pattern to match ascii emoji.
                /// </summary>
            """);
        sw.Write(@"    private const string ASCII_PATTERN = @""");
        for (var i = 0; i < asciiValues.Count; i++) {
            sw.Write(Regex.Escape(asciiValues[i]).Replace("\"", "\"\""));
            if (i < asciiValues.Count - 1) {
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
        var codepoints = emojis
            .SelectMany(e => e.CodePoints.BaseAndFullyQualified)
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(cp => cp.Length)
            .ToList();
        for (var i = 0; i < codepoints.Count; i++) {
            sw.Write(ToSurrogateString(codepoints[i]));
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
        var shortcodes = emojis.SelectMany(x => x.Shortcodes).ToList();
        for (var i = 0; i < shortcodes.Count; i++) {
            if (i > 0) {
                sw.Write("|");
            }
            sw.Write(Regex.Escape(shortcodes[i]).Replace("\"", "\"\""));
        }
        sw.WriteLine(@""";");
        sw.WriteLine();

        sw.WriteLine("""
                /// <summary>
                /// A list of all emoji.
                /// </summary>
            """);
        sw.WriteLine(@"    public static readonly EmojiRecord[] All = new EmojiRecord[] {");

        for (var i = 0; i < emojis.Count; i++) {
            var emoji = emojis[i];

            sw.Write($@"        new (""{EscapeForString(FromCodePoint(emoji.CodePoints.FullyQualified))}"", ""{EscapeForString(emoji.Name)}"", ""{EscapeForString(emoji.Category)}"", ");

            sw.Write($@"new [] {{ ""{emoji.CodePoints.Base}""");
            if (emoji.CodePoints.FullyQualified != emoji.CodePoints.Base) {
                sw.Write($@", ""{emoji.CodePoints.FullyQualified}""");
            }
            sw.Write(" }, ");

            sw.Write($@"new [] {{ ""{EscapeForString(emoji.Shortcodes[0])}""");
            for (var j = 1; j < emoji.Shortcodes.Length; j++) {
                sw.Write($@", ""{EscapeForString(emoji.Shortcodes[j])}""");
            }
            sw.Write(" }, ");

            if (emoji.Ascii != null && emoji.Ascii.Length != 0) {
                sw.Write("new [] { ");
                for (var j = 0; j < emoji.Ascii.Length; j++) {
                    if (j > 0) {
                        sw.Write(", ");
                    }
                    sw.Write($@"""{EscapeForString(emoji.Ascii[j])}""");
                }
                sw.Write(" }, ");
            } else {
                sw.Write("null, ");
            }

            if (emoji.Tags != null && emoji.Tags.Length != 0) {
                sw.Write("new [] { ");
                for (var j = 0; j < emoji.Tags.Length; j++) {
                    if (j > 0) {
                        sw.Write(", ");
                    }
                    sw.Write($@"""{EscapeForString(emoji.Tags[j])}""");
                }
                sw.Write(" }");
            } else {
                sw.Write("null");
            }

            sw.Write($@", {emoji.Version.ToString("G", CultureInfo.InvariantCulture)})");

            if (i < emojis.Count - 1) {
                sw.WriteLine(",");
            }
        }

        sw.WriteLine();
        sw.WriteLine(@"    };");
        sw.WriteLine();
        sw.WriteLine(@"}");
    }

    private static string EscapeForString(string value) {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string BuildFullyQualifiedCodepoint(string normalizedHexcode, int type) {
        if (type != 0 || normalizedHexcode.Split('-').Contains("fe0f")) {
            return normalizedHexcode;
        }
        return normalizedHexcode + "-fe0f";
    }

    private static string BuildBaseCodepoint(string fullyQualifiedCodepoint) {
        var tokens = fullyQualifiedCodepoint
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t != "200d" && t != "fe0f")
            .ToArray();

        if (tokens.Length == 0) {
            throw new InvalidDataException("Unable to build base codepoint from " + fullyQualifiedCodepoint);
        }

        return string.Join("-", tokens);
    }

    private static string NormalizeHexcode(string hexcode) {
        return string.Join("-", hexcode
            .Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant().PadLeft(4, '0')));
    }

    private static bool IsRegionalIndicator(string normalizedHexcode) {
        if (normalizedHexcode.Contains('-')) {
            return false;
        }

        if (!uint.TryParse(normalizedHexcode, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codepoint)) {
            return false;
        }

        return codepoint >= 0x1F1E6 && codepoint <= 0x1F1FF;
    }

    private static void EnsureFileExists(string path) {
        if (!File.Exists(path)) {
            throw new FileNotFoundException(
                $"Missing required data file: {path}. Run `npm install` in the repository root to install emojibase-data.");
        }
    }

    private static string FindRepositoryRoot() {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null) {
            var generatorPath = Path.Combine(dir.FullName, "src", "Generator", "Generator.csproj");
            var toolkitPath = Path.Combine(dir.FullName, "src", "EmojiToolkit", "Emoji.cs");
            if (File.Exists(generatorPath) && File.Exists(toolkitPath)) {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root from current directory.");
    }

    /// <summary>
    /// Convert a unicode character to its code point/code pair(s)
    /// </summary>
    private static string ToCodePoint(string unicode) {
        var codepoint = "";
        for (var i = 0; i < unicode.Length; i += char.IsSurrogatePair(unicode, i) ? 2 : 1) {
            if (i > 0) {
                codepoint += "-";
            }
            codepoint += string.Format("{0:x4}", char.ConvertToUtf32(unicode, i));
        }
        return codepoint.ToLowerInvariant();
    }

    /// <summary>
    /// Converts a codepoint to unicode surrogate pairs.
    /// </summary>
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
    internal static string FromCodePoint(string codepoint) {
        var bytes = AsUtf16Bytes(codepoint).ToArray();
        return Encoding.Unicode.GetString(bytes);
    }

    // Little Endian byte order
    public static IEnumerable<byte> AsUtf16Bytes(string codepoint) {
        var codepoints = codepoint.Split('-').Select(x => uint.Parse(x, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat));
        var bytes = new byte[4];
        foreach (var cp in codepoints) {
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

record EmojibaseEntry {
    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("hexcode")]
    public string Hexcode { get; set; } = "";

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = [];

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("version")]
    public double Version { get; set; }

    [JsonPropertyName("group")]
    public int? Group { get; set; }

    [JsonPropertyName("subgroup")]
    public int? Subgroup { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("emoticon")]
    public JsonElement Emoticon { get; set; }

    [JsonPropertyName("skins")]
    public EmojibaseSkin[] Skins { get; set; } = [];
}

record EmojibaseSkin {
    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("hexcode")]
    public string Hexcode { get; set; } = "";

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = [];

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("version")]
    public double Version { get; set; }

    [JsonPropertyName("group")]
    public int? Group { get; set; }

    [JsonPropertyName("subgroup")]
    public int? Subgroup { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("emoticon")]
    public JsonElement Emoticon { get; set; }
}

record EmojiMessages {
    [JsonPropertyName("groups")]
    public EmojiMessage[] Groups { get; set; } = [];
}

record EmojiMessage {
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

record GeneratedEmoji(
    string Name,
    string Category,
    CodePoints CodePoints,
    string[] Shortcodes,
    string[] Ascii,
    string[] Tags,
    double Version,
    int? Order,
    int Index);

record CodePoints(string Base, string FullyQualified) {
    public string[] BaseAndFullyQualified => Base == FullyQualified ? [Base] : [Base, FullyQualified];
}
