using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmojiToolkit.Tests;

[TestClass]
public class EmojiTests {

    [TestMethod]
    public void All() {
        Assert.AreEqual(3659, Emoji.All.Length);
    }

    [TestMethod]
    public void Ascii() {
        Assert.AreEqual(":)", Emoji.Ascii(":slight_smile:"));
        Assert.AreEqual(":D", Emoji.Ascii("š"));
        Assert.IsNull(Emoji.Ascii(":poop:"));
    }

    [TestMethod]
    public void Asciify() {
        string text = ":poop: :slight_smile: šŗ š";
        string expected = ":poop: :) šŗ :D";
        string actual = Emoji.Asciify(text);
        Assert.AreEqual(expected, actual);

        text = "poop:)";
        expected = text;
        actual = Emoji.Asciify(text);
        Assert.AreEqual(expected, actual);

        text = $@"Emoji in <img alt=""š"" src=""/img.png"" /> shouf not be replaced";
        expected = text;
        actual = Emoji.Asciify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Demojify() {
        // to short
        string text = "Hello world! š :smile:";
        string expected = "Hello world! :smile: :smile:";
        string actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // single unicode character conversion
        text = "š";
        expected = ":snail:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = "š½ is not :alien: and ģ  is not š½ or š½";
        expected = ":alien: is not :alien: and ģ  is not :alien: or :alien:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = "š\nš";
        expected = ":dancer:\n:dancer:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // single character with surrogate pair
        text = "9ā£";
        expected = ":nine:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character mid sentence
        text = "The š¦ is EmojiToolkit's official mascot.";
        expected = "The :unicorn: is EmojiToolkit's official mascot.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character mid sentence with a comma
        text = "The š¦, is EmojiToolkit's official mascot.";
        expected = "The :unicorn:, is EmojiToolkit's official mascot.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at start of sentence
        text = "š mail.";
        expected = ":snail: mail.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at start of sentence with apostrophe
        text = "š's are cool!";
        expected = ":snail:'s are cool!";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at end of sentence
        text = "EmojiToolkit's official mascot is š¦.";
        expected = "EmojiToolkit's official mascot is :unicorn:.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at end of sentence with alternate puncuation
        text = "EmojiToolkit's official mascot is š¦!";
        expected = "EmojiToolkit's official mascot is :unicorn:!";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at end of sentence with preceeding colon
        text = "EmojiToolkit's official mascot: š¦";
        expected = "EmojiToolkit's official mascot: :unicorn:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character inside of IMG tag
        text = $@"The <img class=""emoji"" alt=""š¦"" src=""/emoji/1f984.png"" /> is EmojiToolkit's official mascot";
        expected = text;
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // unicode alternate to short
        text = "#ļøā£"; // 0023-fe0f-20e3
        expected = ":hash:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // unicode alternates
        text = "ā¤ļø and ā„ļø"; // 2764-fe0f and 2665-fe0f
        expected = ":heart: and :hearts:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DiversityShortcodes() {
        string shortcode = Emoji.Shortcode("š");
        Assert.AreEqual(":thumbsup:", shortcode);

        shortcode = Emoji.Shortcode("šš»");
        Assert.AreEqual(":thumbsup_tone1:", shortcode);

        shortcode = Emoji.Shortcode("ššæ");
        Assert.AreEqual(":thumbsup_tone5:", shortcode);
    }

    [TestMethod]
    public void Emojify() {
        // shortname to unicode
        string text = "Hello world! š :smile:";
        string expected = "Hello world! š š";
        string actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // single shortname
        text = ":snail:";
        expected = "š";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname mid sentence with a comma
        text = "The :unicorn:, is EmojiToolkit's official mascot.";
        expected = "The š¦, is EmojiToolkit's official mascot.";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence
        text = ":snail: mail.";
        expected = "š mail.";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence with apostrophe
        text = ":snail:'s are cool!";
        expected = "š's are cool!";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname shares a colon
        text = ":invalid:snail:";
        expected = ":invalidš";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = ":alien: is š½ and ģ  is not :alien: or :alien: also :randomy: is not emoji";
        expected = "š½ is š½ and ģ  is not š½ or š½ also :randomy: is not emoji";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = ":dancer:\n:dancer:";
        expected = "š\nš";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // triple emoji string
        text = ":blush::ok_hand::two_hearts:";
        expected = "ššš";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at end of sentence
        text = "EmojiToolkit's official mascot is :unicorn:.";
        expected = "EmojiToolkit's official mascot is š¦.";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at end of sentence with alternate punctuation
        text = "EmojiToolkit's official mascot is :unicorn:!";
        expected = "EmojiToolkit's official mascot is š¦!";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at end of sentence with preceeding colon
        text = "EmojiToolkit's official mascot: :unicorn:";
        expected = "EmojiToolkit's official mascot: š¦";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname inside of IMG tag
        text = $@"The <img class=""emoji"" alt="":unicorn:"" src=""/emoji/1f984.png"" /> is EmojiToolkit's official mascot.";
        expected = text;
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname to unicode with code pairs
        text = ":nine:";
        expected = "9ļøā£";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname alias
        text = ":poo:";
        expected = "š©";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void EmojifyAscii() {
        // single smiley
        string text = ":D";
        string expected = "š";
        string actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // single smiley with incorrect case (shouldn't convert)
        text = ":d";
        expected = text;
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // multiple smileys
        text = ";) :P :* :)";
        expected = "š š š š";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to start a sentence
        text = @":\ is our confused smiley.";
        expected = "š is our confused smiley.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to end a sentence
        text = "Our smiley to represent joy is :')";
        expected = "Our smiley to represent joy is š";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to end a sentence with puncuation
        text = "The reverse to the joy smiley is the cry smiley :'(.";
        expected = "The reverse to the joy smiley is the cry smiley š¢.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to end a sentence with preceeding punctuation
        text = @"This is the ""flushed"" smiley: :$.";
        expected = @"This is the ""flushed"" smiley: š³.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley inside of an IMG tag (shouldn't convert anything inside of the tag)
        text = $@"Smile <img class=""emoji"" alt="":)"" src=""/emoji/1f604.png"" /> because it's going to be a good day.";
        expected = text;
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // typical username password fail  (shouldn't convert the user:pass, but should convert the last :P)
        text = @"Please log-in with user:pass as your credentials :P.";
        expected = @"Please log-in with user:pass as your credentials š.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // shouldn't replace an ascii smiley in a URL (shouldn't replace :/)
        text = @"Check out http://www.example.com";
        expected = text;
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // mixed unicode, shortname and ascii conversion
        text = "š :smile: :D";
        expected = "š š š";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Find() {
        // fid emoji by name
        var emoji = Emoji.Find("party popper");
        Assert.AreEqual("š", emoji.Single().Raw);

        // find emoji by category
        emoji = Emoji.Find("objects");
        Assert.IsTrue(emoji.Any(e => e.Raw == "š"));

        // find emoji by shortcode
        emoji = Emoji.Find(":party");
        Assert.IsTrue(emoji.Any(e => e.Raw == "š"));

        // find emoji by tag
        emoji = Emoji.Find("celebration");
        Assert.IsTrue(emoji.Any(e => e.Raw == "š"));
    }

    [TestMethod]
    public void GetAlternate() {
        var emoji = Emoji.Get(":relaxed:");
        var cp1 = emoji.Codepoints[0]; // base code point
        var cp2 = emoji.Codepoints[1]; // fully qualified code point
        Assert.AreNotEqual(cp1, cp2);

        var raw1 = Emoji.FromCodePoint(cp1);
        var raw2 = Emoji.FromCodePoint(cp2);
        Assert.AreNotEqual(raw1, raw2);

        // raw should not use base codepoint
        var e1 = Emoji.Get(raw1);
        Assert.AreNotEqual(raw1, e1.Raw);

        // raw should use fully qualified codepoint
        var e2 = Emoji.Get(raw2);        
        Assert.AreEqual(raw2, e2.Raw);

        // both codepoints should resolve to the same emoji
        Assert.AreEqual(e1.Name, e2.Name);
    }

    [TestMethod]
    public void Image() {
        Assert.AreEqual(@"<img class=""emoji"" alt=""š"" title="":snail:"" src=""/emoji/1f40c.png"" />", Emoji.Image(":snail:"));
        Assert.AreEqual(@"<img class=""emoji"" alt=""š"" title="":snail:"" src=""/e/1f40c.svg"" />", Emoji.Image("š", path: "/e/", ext: ".svg"));
    }

    [TestMethod]
    public void Imagify() {
        // mixed unicode, shortname and ascii
        string text = "Hello š :smile: world :D";
        string expected = $@"Hello <img class=""emoji"" alt=""š"" title="":smile:"" src=""/emoji/1f604.png"" /> <img class=""emoji"" alt=""š"" title="":smile:"" src=""/emoji/1f604.png"" /> world <img class=""emoji"" alt=""š"" title="":smile:"" src=""/emoji/1f604.png"" />";
        string actual = Emoji.Imagify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence with apostrophe
        text = ":snail:'s are cool!";
        expected = $@"<img class=""emoji"" alt=""š"" title="":snail:"" src=""/emoji/1f40c.png"" />'s are cool!";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // shortname shares a colon
        text = ":invalid:snail:";
        expected = $@":invalid<img class=""emoji"" alt=""š"" title="":snail:"" src=""/emoji/1f40c.png"" />";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = ":alien: is š½ and ģ  is not :alien: or :alien: also :randomy: is not emoji";
        expected = """<img class="emoji" alt="š½" title=":alien:" src="/emoji/1f47d.png" /> is <img class="emoji" alt="š½" title=":alien:" src="/emoji/1f47d.png" /> and ģ  is not <img class="emoji" alt="š½" title=":alien:" src="/emoji/1f47d.png" /> or <img class="emoji" alt="š½" title=":alien:" src="/emoji/1f47d.png" /> also :randomy: is not emoji""";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = ":dancer:\n:dancer:";
        expected = $"<img class=\"emoji\" alt=\"š\" title=\":dancer:\" src=\"/emoji/1f483.png\" />\n<img class=\"emoji\" alt=\"š\" title=\":dancer:\" src=\"/emoji/1f483.png\" />";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // triple emoji string
        text = ":blush::ok_hand::two_hearts:";
        expected = $@"<img class=""emoji"" alt=""š"" title="":blush:"" src=""/emoji/1f60a.png"" /><img class=""emoji"" alt=""š"" title="":ok_hand:"" src=""/emoji/1f44c.png"" /><img class=""emoji"" alt=""š"" title="":two_hearts:"" src=""/emoji/1f495.png"" />";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    public void Spanify() {
        // mixed unicode, shortname and ascii
        string text = "Hello š :smile: world :D";
        string expected = $@"Hello <span class=""emoji"" title="":smile:"">š</span> <span class=""emoji"" title="":smile:"">š</span> world <span class=""emoji"" title="":smile:"">š</span>";
        string actual = Emoji.Spanify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence with apostrophe
        text = ":snail:'s are cool!";
        expected = $@"<span class=""emoji"" title="":snail:"">š</span>'s are cool!";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // shortname shares a colon
        text = ":invalid:snail:";
        expected = $@":invalid<span class=""emoji"" title="":snail:"">š</span>";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = ":alien: is š½ and ģ  is not :alien: or :alien: also :randomy: is not emoji";
        expected = """<span class="emoji" title=":alien:">š½</span> is <span class="emoji" title=":alien:">š½</span> and ģ  is not <span class="emoji" title=":alien:">š½</span> or <span class="emoji" title=":alien:">š½</span> also :randomy: is not emoji""";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = ":dancer:\n:dancer:";
        expected = $"<span class=\"emoji\" title=\":dancer:\">š</span>\n<span class=\"emoji\" title=\":dancer:\">š</span>";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // triple emoji string
        text = ":blush::ok_hand::two_hearts:";
        expected = $@"<span class=""emoji"" title="":blush:"">š</span><span class=""emoji"" title="":ok_hand:"">š</span><span class=""emoji"" title="":two_hearts:"">š</span>";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void IsEmoji() {
        // singular emoji
        Assert.IsTrue(Emoji.IsEmoji("š"));
        Assert.IsTrue(Emoji.IsEmoji("š", 1));
        Assert.IsFalse(Emoji.IsEmoji("šš", 1));
        Assert.IsTrue(Emoji.IsEmoji("šš", 2));

        // combined emoji
        Assert.IsTrue(Emoji.IsEmoji("š¤·āāļø"));
        Assert.IsTrue(Emoji.IsEmoji("š¤·āāļø", 1));

        // mixed emoji and text
        Assert.IsFalse(Emoji.IsEmoji("ašb"));

        // skintones
        Assert.IsTrue(Emoji.IsEmoji("šš»"));
        Assert.IsTrue(Emoji.IsEmoji("šš»", 1));

        // no emoji
        Assert.IsFalse(Emoji.IsEmoji("ab c"));

        // digits and symbols are not emoji
        Assert.IsFalse(Emoji.IsEmoji("1"));
        Assert.IsFalse(Emoji.IsEmoji("*"));

        // digits ands symbols followed by the variation selector are emoji
        Assert.IsTrue(Emoji.IsEmoji("1ļøā£"));
        Assert.IsTrue(Emoji.IsEmoji("*ļøā£"));

        // punctuation is not emoji
        Assert.IsFalse(Emoji.IsEmoji("."));
        Assert.IsFalse(Emoji.IsEmoji("!"));
        Assert.IsFalse(Emoji.IsEmoji("?"));
        Assert.IsFalse(Emoji.IsEmoji("#"));
        Assert.IsFalse(Emoji.IsEmoji("*"));

        // whitespace is allowed
        Assert.IsTrue(Emoji.IsEmoji("š š"));
    }

    [TestMethod]
    public void ReadmeTest() {
        // gets an emoji by shortcode
        var raw = Emoji.Get(":smiley:").Raw;
        Assert.AreEqual("š", raw);

        // gets an emoji by raw unicode string
        var name = Emoji.Get("š").Name;
        Assert.AreEqual("grinning face with big eyes", name);

        // gets the ascii equivalent of an emoji
        var ascii = Emoji.Ascii("š");
        Assert.AreEqual(";)", ascii);

        // gets <img> tag for the specified emoji
        var img = Emoji.Image(":wink:");
        Assert.AreEqual("""<img class="emoji" alt="š" title=":wink:" src="/emoji/1f609.png" />""", img);

        // gets the raw unicode equivalent for an emoji shortcode
        raw = Emoji.Raw(":smiley:");
        Assert.AreEqual("š", raw);

        // gets the shortcode for a raw unicode string
        var shortcode = Emoji.Shortcode("š");
        Assert.AreEqual(":smiley:", shortcode);

        // gets <span> tag for the specified emoji
        var span = Emoji.Span(":wink:");
        Assert.AreEqual("""<span class="emoji" title=":wink:">š</span>""", span);

        // replaces emoji shortcodes and raw unicode strings with their ascii equivalents
        var asciified = Emoji.Asciify("š :wink:");
        Assert.AreEqual(";) ;)", asciified);

        // replaces emoji shortcodes with raw unicode strings.
        var emojified = Emoji.Emojify("it's raining :cat:s and :dog:s!");
        Assert.AreEqual("it's raining š±s and š¶s!", emojified);

        // replaces raw unicode strings with emoji shortcodes
        var demojified = Emoji.Demojify("it's raining š±s and š¶s!");
        Assert.AreEqual("it's raining :cat:s and :dog:s!", demojified);

        // replaces emoji shortcodes and raw unicode strings with <img> tags
        var imagified = Emoji.Imagify("it's raining :cat:s and š¶s!");
        Assert.AreEqual("""it's raining <img class="emoji" alt="š±" title=":cat:" src="/emoji/1f431.png" />s and <img class="emoji" alt="š¶" title=":dog:" src="/emoji/1f436.png" />s!""", imagified);

        // replaces emoji shortcodes and raw unicode strings with <span> tags
        var spanified = Emoji.Spanify("it's raining :cat:s and š¶s!");
        Assert.AreEqual("""it's raining <span class="emoji" title=":cat:">š±</span>s and <span class="emoji" title=":dog:">š¶</span>s!""", spanified);

        // returns emoji with matching name, category, shortcodes or tags
        var emoji = Emoji.Find("smile").First();
        Assert.AreEqual("š", emoji.Raw);

        // determines whether a string is comprised solely of emoji
        Assert.IsTrue(Emoji.IsEmoji("š±š¶"));
        Assert.IsFalse(Emoji.IsEmoji("it's raining š±s and š¶s!"));
    }

    [TestMethod]
    public void SymbolsAndDigitsAreNotEmoji() {
        string text = @" !""#$%&'()*+,-./0123456789:;<=>?@";
        string actual = Emoji.Demojify(text);
        Assert.AreEqual(text, actual);
    }

    [TestMethod]
    public void ToCodepoint() {
        // :grinning:
        string unicode = "š";
        string codepoint = Emoji.ToCodePoint(unicode);
        Assert.AreEqual("1f600", codepoint);

        var actual = Emoji.FromCodePoint(codepoint);
        Assert.AreEqual(unicode, actual);

        var surrogate = Emoji.ToSurrogate(codepoint);
        Assert.AreEqual("\\ud83d\\ude00", surrogate);

        // unicode alternate
        unicode = "ā¤ļø";
        codepoint = Emoji.ToCodePoint(unicode);
        Assert.AreEqual("2764-fe0f", codepoint);
        actual = Emoji.FromCodePoint(codepoint);
        Assert.AreEqual(unicode, actual);

        var emoji = Emoji.Get(":family_mwgb:");
        codepoint = emoji.Codepoints[1];
        actual = Emoji.FromCodePoint(codepoint);
        Assert.AreEqual(emoji.Raw, actual);
    }

    [TestMethod]
    public void Version11Emoji() {
        var emoji = Emoji.Get("š„¶");
        Assert.AreEqual(":cold_face:", emoji.Shortcodes[0]);
    }

    [TestMethod]
    public void Version12Emoji() {
        var emoji = Emoji.Get("š„±");
        Assert.AreEqual(":yawning_face:", emoji.Shortcodes[0]);
    }

    [TestMethod]
    public void Version13Emoji() {
        Assert.AreEqual(117, Emoji.All.Count(e => e.Version == "13"));
        var emoji = Emoji.Get("š„²");
        Assert.AreEqual(":smiling_face_with_tear:", emoji.Shortcodes[0]);
    }

    [TestMethod]
    public void Version14Emoji() {
        Assert.AreEqual(112, Emoji.All.Count(e => e.Version == "14"));
        Assert.IsNotNull(Emoji.Get(":melting_face:"));
    }

    [TestMethod]
    public void ZWJSequence() {
        // :family: is a not a ZWJ sequence
        string raw = "šŖ";

        string codepoint = Emoji.ToCodePoint(raw);
        Assert.AreEqual("1f46a", codepoint);

        string shortcode = Emoji.Shortcode(raw);
        Assert.AreEqual(":family:", shortcode);

        // :family_mwgb: is a ZWJ sequence combining :man: (š¤·), :woman:, :girl: and :boy:
        raw = "šØāš©āš§āš¦";
        codepoint = Emoji.ToCodePoint(raw);
        Assert.AreEqual("1f468-200d-1f469-200d-1f467-200d-1f466", codepoint);

        shortcode = Emoji.Shortcode(raw);
        Assert.AreNotEqual(":man:ā:woman:ā:girl:ā:boy:", shortcode);
        Assert.AreEqual(":family_mwgb:", shortcode);

        // :man_shrugging: is a ZWJ sequence combining :person_shrugging: (š¤·), Zero Width Joiner (ZWJ) and :male_sign: (āļø)           
        raw = "š¤·āāļø";
        codepoint = Emoji.ToCodePoint(raw);
        Assert.AreEqual("1f937-200d-2642-fe0f", codepoint);

        shortcode = Emoji.Shortcode(raw);
        Assert.AreNotEqual(":person_shrugging:ā:male_sign:", shortcode);
        Assert.AreEqual(":man_shrugging:", shortcode);

        string surrogate = Emoji.ToSurrogate(codepoint);
        Assert.AreNotEqual(@"\ud83e\udd37", surrogate);
        Assert.AreEqual(@"\ud83e\udd37\u200d\u2642\ufe0f", surrogate);
    }
}
