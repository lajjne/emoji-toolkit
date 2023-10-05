using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmojiToolkit.Tests;

[TestClass]
public class EmojiTests {

    [TestMethod]
    public void All() {
        Assert.AreEqual(3690, Emoji.All.Length);
    }

    [TestMethod]
    public void Ascii() {
        Assert.AreEqual(":)", Emoji.Ascii(":slight_smile:"));
        Assert.AreEqual(":D", Emoji.Ascii("😄"));
        Assert.IsNull(Emoji.Ascii(":poop:"));
    }

    [TestMethod]
    public void Asciify() {
        var text = ":poop: :slight_smile: 🍺 😄";
        var expected = ":poop: :) 🍺 :D";
        var actual = Emoji.Asciify(text);
        Assert.AreEqual(expected, actual);

        text = "poop:)";
        expected = text;
        actual = Emoji.Asciify(text);
        Assert.AreEqual(expected, actual);

        text = $@"Emoji in <img alt=""😄"" src=""/img.png"" /> shouf not be replaced";
        expected = text;
        actual = Emoji.Asciify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Demojify() {
        // to short
        var text = "Hello world! 😄 :smile:";
        var expected = "Hello world! :smile: :smile:";
        var actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // single unicode character conversion
        text = "🐌";
        expected = ":snail:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = "👽 is not :alien: and 저 is not 👽 or 👽";
        expected = ":alien: is not :alien: and 저 is not :alien: or :alien:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = "💃\n💃";
        expected = ":dancer:\n:dancer:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // single character with surrogate pair
        text = "9⃣";
        expected = ":nine:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character mid sentence
        text = "The 🦄 is EmojiToolkit's official mascot.";
        expected = "The :unicorn: is EmojiToolkit's official mascot.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character mid sentence with a comma
        text = "The 🦄, is EmojiToolkit's official mascot.";
        expected = "The :unicorn:, is EmojiToolkit's official mascot.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at start of sentence
        text = "🐌 mail.";
        expected = ":snail: mail.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at start of sentence with apostrophe
        text = "🐌's are cool!";
        expected = ":snail:'s are cool!";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at end of sentence
        text = "EmojiToolkit's official mascot is 🦄.";
        expected = "EmojiToolkit's official mascot is :unicorn:.";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at end of sentence with alternate puncuation
        text = "EmojiToolkit's official mascot is 🦄!";
        expected = "EmojiToolkit's official mascot is :unicorn:!";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character at end of sentence with preceeding colon
        text = "EmojiToolkit's official mascot: 🦄";
        expected = "EmojiToolkit's official mascot: :unicorn:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // character inside of IMG tag
        text = $@"The <img class=""emoji"" alt=""🦄"" src=""/emoji/1f984.png"" /> is EmojiToolkit's official mascot";
        expected = text;
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // unicode alternate to short
        text = "#️⃣"; // 0023-fe0f-20e3
        expected = ":hash:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);

        // unicode alternates
        text = "❤️ and ♥️"; // 2764-fe0f and 2665-fe0f
        expected = ":heart: and :hearts:";
        actual = Emoji.Demojify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DiversityShortcodes() {
        var shortcode = Emoji.Shortcode("👍");
        Assert.AreEqual(":thumbsup:", shortcode);

        shortcode = Emoji.Shortcode("👍🏻");
        Assert.AreEqual(":thumbsup_tone1:", shortcode);

        shortcode = Emoji.Shortcode("👍🏿");
        Assert.AreEqual(":thumbsup_tone5:", shortcode);
    }

    [TestMethod]
    public void Emojify() {
        // shortname to unicode
        var text = "Hello world! 😄 :smile:";
        var expected = "Hello world! 😄 😄";
        var actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // single shortname
        text = ":snail:";
        expected = "🐌";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname mid sentence with a comma
        text = "The :unicorn:, is EmojiToolkit's official mascot.";
        expected = "The 🦄, is EmojiToolkit's official mascot.";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence
        text = ":snail: mail.";
        expected = "🐌 mail.";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence with apostrophe
        text = ":snail:'s are cool!";
        expected = "🐌's are cool!";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname shares a colon
        text = ":invalid:snail:";
        expected = ":invalid🐌";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = ":alien: is 👽 and 저 is not :alien: or :alien: also :randomy: is not emoji";
        expected = "👽 is 👽 and 저 is not 👽 or 👽 also :randomy: is not emoji";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = ":dancer:\n:dancer:";
        expected = "💃\n💃";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // triple emoji string
        text = ":blush::ok_hand::two_hearts:";
        expected = "😊👌💕";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at end of sentence
        text = "EmojiToolkit's official mascot is :unicorn:.";
        expected = "EmojiToolkit's official mascot is 🦄.";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at end of sentence with alternate punctuation
        text = "EmojiToolkit's official mascot is :unicorn:!";
        expected = "EmojiToolkit's official mascot is 🦄!";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname at end of sentence with preceeding colon
        text = "EmojiToolkit's official mascot: :unicorn:";
        expected = "EmojiToolkit's official mascot: 🦄";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname inside of IMG tag
        text = $@"The <img class=""emoji"" alt="":unicorn:"" src=""/emoji/1f984.png"" /> is EmojiToolkit's official mascot.";
        expected = text;
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname to unicode with code pairs
        text = ":nine:";
        expected = "9️⃣";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);

        // shortname alias
        text = ":poo:";
        expected = "💩";
        actual = Emoji.Emojify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void EmojifyAscii() {
        // single smiley
        var text = ":D";
        var expected = "😄";
        var actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // single smiley with incorrect case (shouldn't convert)
        text = ":d";
        expected = text;
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // multiple smileys
        text = ";) :P :* :)";
        expected = "😉 😛 😘 🙂";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to start a sentence
        text = @":\ is our confused smiley.";
        expected = "😕 is our confused smiley.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to end a sentence
        text = "Our smiley to represent joy is :')";
        expected = "Our smiley to represent joy is 😂";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to end a sentence with puncuation
        text = "The reverse to the joy smiley is the cry smiley :'(.";
        expected = "The reverse to the joy smiley is the cry smiley 😢.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley to end a sentence with preceeding punctuation
        text = @"This is the ""flushed"" smiley: :$.";
        expected = @"This is the ""flushed"" smiley: 😳.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // smiley inside of an IMG tag (shouldn't convert anything inside of the tag)
        text = $@"Smile <img class=""emoji"" alt="":)"" src=""/emoji/1f604.png"" /> because it's going to be a good day.";
        expected = text;
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // typical username password fail  (shouldn't convert the user:pass, but should convert the last :P)
        text = @"Please log-in with user:pass as your credentials :P.";
        expected = @"Please log-in with user:pass as your credentials 😛.";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // shouldn't replace an ascii smiley in a URL (shouldn't replace :/)
        text = @"Check out http://www.example.com";
        expected = text;
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // mixed unicode, shortname and ascii conversion
        text = "😄 :smile: :D";
        expected = "😄 😄 😄";
        actual = Emoji.Emojify(text, ascii: true);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Find() {
        // fid emoji by name
        var emoji = Emoji.Find("party popper");
        Assert.AreEqual("🎉", emoji.Single().Raw);

        // find emoji by category
        emoji = Emoji.Find("objects");
        Assert.IsTrue(emoji.Any(e => e.Raw == "🎉"));

        // find emoji by shortcode
        emoji = Emoji.Find(":party");
        Assert.IsTrue(emoji.Any(e => e.Raw == "🎉"));

        // find emoji by tag
        emoji = Emoji.Find("celebration");
        Assert.IsTrue(emoji.Any(e => e.Raw == "🎉"));
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
    public void GetAscii() {
        var emoji = Emoji.Get(":D");
        Assert.AreEqual("😄", emoji.Raw);
    }

    [TestMethod]
    public void Image() {
        Assert.AreEqual(@"<img class=""emoji"" alt=""🐌"" title="":snail:"" src=""/emoji/1f40c.png"" />", Emoji.Image(":snail:"));
        Assert.AreEqual(@"<img class=""emoji"" alt=""🐌"" title="":snail:"" src=""/e/1f40c.svg"" />", Emoji.Image("🐌", path: "/e/", ext: ".svg"));
        Assert.AreEqual(@"<img class=""emo"" alt=""🐌"" title="":snail:"" src=""/e/1f40c.svg"" />", Emoji.Image("🐌", css: "emo", path: "/e/", ext: ".svg"));
    }

    [TestMethod]
    public void Imagify() {
        // mixed unicode, shortname and ascii
        var text = "Hello 😄 :smile: world :D";
        var expected = $@"Hello <img class=""emoji"" alt=""😄"" title="":smile:"" src=""/emoji/1f604.png"" /> <img class=""emoji"" alt=""😄"" title="":smile:"" src=""/emoji/1f604.png"" /> world <img class=""emoji"" alt=""😄"" title="":smile:"" src=""/emoji/1f604.png"" />";
        var actual = Emoji.Imagify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence with apostrophe
        text = ":snail:'s are cool!";
        expected = $@"<img class=""emoji"" alt=""🐌"" title="":snail:"" src=""/emoji/1f40c.png"" />'s are cool!";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // shortname shares a colon
        text = ":invalid:snail:";
        expected = $@":invalid<img class=""emoji"" alt=""🐌"" title="":snail:"" src=""/emoji/1f40c.png"" />";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = ":alien: is 👽 and 저 is not :alien: or :alien: also :randomy: is not emoji";
        expected = """<img class="emoji" alt="👽" title=":alien:" src="/emoji/1f47d.png" /> is <img class="emoji" alt="👽" title=":alien:" src="/emoji/1f47d.png" /> and 저 is not <img class="emoji" alt="👽" title=":alien:" src="/emoji/1f47d.png" /> or <img class="emoji" alt="👽" title=":alien:" src="/emoji/1f47d.png" /> also :randomy: is not emoji""";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = ":dancer:\n:dancer:";
        expected = $"<img class=\"emoji\" alt=\"💃\" title=\":dancer:\" src=\"/emoji/1f483.png\" />\n<img class=\"emoji\" alt=\"💃\" title=\":dancer:\" src=\"/emoji/1f483.png\" />";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);

        // triple emoji string
        text = ":blush::ok_hand::two_hearts:";
        expected = $@"<img class=""emoji"" alt=""😊"" title="":blush:"" src=""/emoji/1f60a.png"" /><img class=""emoji"" alt=""👌"" title="":ok_hand:"" src=""/emoji/1f44c.png"" /><img class=""emoji"" alt=""💕"" title="":two_hearts:"" src=""/emoji/1f495.png"" />";
        actual = Emoji.Imagify(text);
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    public void Spanify() {
        // mixed unicode, shortname and ascii
        var text = "Hello 😄 :smile: world :D";
        var expected = $@"Hello <span class=""emoji"" title="":smile:"">😄</span> <span class=""emoji"" title="":smile:"">😄</span> world <span class=""emoji"" title="":smile:"">😄</span>";
        var actual = Emoji.Spanify(text, ascii: true);
        Assert.AreEqual(expected, actual);

        // shortname at start of sentence with apostrophe
        text = ":snail:'s are cool!";
        expected = $@"<span class=""emoji"" title="":snail:"">🐌</span>'s are cool!";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // shortname shares a colon
        text = ":invalid:snail:";
        expected = $@":invalid<span class=""emoji"" title="":snail:"">🐌</span>";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // mixed ascii, regular unicode and duplicate emoji
        text = ":alien: is 👽 and 저 is not :alien: or :alien: also :randomy: is not emoji";
        expected = """<span class="emoji" title=":alien:">👽</span> is <span class="emoji" title=":alien:">👽</span> and 저 is not <span class="emoji" title=":alien:">👽</span> or <span class="emoji" title=":alien:">👽</span> also :randomy: is not emoji""";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // multiline emoji string
        text = ":dancer:\n:dancer:";
        expected = $"<span class=\"emoji\" title=\":dancer:\">💃</span>\n<span class=\"emoji\" title=\":dancer:\">💃</span>";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);

        // triple emoji string
        text = ":blush::ok_hand::two_hearts:";
        expected = $@"<span class=""emoji"" title="":blush:"">😊</span><span class=""emoji"" title="":ok_hand:"">👌</span><span class=""emoji"" title="":two_hearts:"">💕</span>";
        actual = Emoji.Spanify(text);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void IsEmoji() {
        // singular emoji
        Assert.IsTrue(Emoji.IsEmoji("😀"));
        Assert.IsTrue(Emoji.IsEmoji("😀", 1));
        Assert.IsFalse(Emoji.IsEmoji("😀😀", 1));
        Assert.IsTrue(Emoji.IsEmoji("😀😀", 2));

        // combined emoji
        Assert.IsTrue(Emoji.IsEmoji("🤷‍♂️"));
        Assert.IsTrue(Emoji.IsEmoji("🤷‍♂️", 1));

        // mixed emoji and text
        Assert.IsFalse(Emoji.IsEmoji("a😀b"));

        // skintones
        Assert.IsTrue(Emoji.IsEmoji("👍🏻"));
        Assert.IsTrue(Emoji.IsEmoji("👍🏻", 1));

        // no emoji
        Assert.IsFalse(Emoji.IsEmoji("ab c"));

        // digits and symbols are not emoji
        Assert.IsFalse(Emoji.IsEmoji("1"));
        Assert.IsFalse(Emoji.IsEmoji("*"));

        // digits ands symbols followed by the variation selector are emoji
        Assert.IsTrue(Emoji.IsEmoji("1️⃣"));
        Assert.IsTrue(Emoji.IsEmoji("*️⃣"));

        // punctuation is not emoji
        Assert.IsFalse(Emoji.IsEmoji("."));
        Assert.IsFalse(Emoji.IsEmoji("!"));
        Assert.IsFalse(Emoji.IsEmoji("?"));
        Assert.IsFalse(Emoji.IsEmoji("#"));
        Assert.IsFalse(Emoji.IsEmoji("*"));

        // whitespace is allowed
        Assert.IsTrue(Emoji.IsEmoji("😀 😀"));
    }

    [TestMethod]
    public void ReadmeTest() {
        // gets an emoji by shortcode
        var raw = Emoji.Get(":smiley:").Raw;
        Assert.AreEqual("😃", raw);

        //  get an emoji by ascii equivalent
        raw = Emoji.Get(":-D").Raw;
        Assert.AreEqual("😃", raw);

        // gets an emoji by raw unicode string
        var name = Emoji.Get("😃").Name;
        Assert.AreEqual("grinning face with big eyes", name);

        // gets the ascii equivalent of an emoji
        var ascii = Emoji.Ascii("😉");
        Assert.AreEqual(";)", ascii);

        // gets <img> tag for the specified emoji
        var img = Emoji.Image(":wink:");
        Assert.AreEqual("""<img class="emoji" alt="😉" title=":wink:" src="/emoji/1f609.png" />""", img);

        // gets the raw unicode equivalent for an emoji shortcode
        raw = Emoji.Raw(":smiley:");
        Assert.AreEqual("😃", raw);

        // gets the shortcode for a raw unicode string
        var shortcode = Emoji.Shortcode("😃");
        Assert.AreEqual(":smiley:", shortcode);

        // gets <span> tag for the specified emoji
        var span = Emoji.Span(":wink:");
        Assert.AreEqual("""<span class="emoji" title=":wink:">😉</span>""", span);

        // replaces emoji shortcodes and raw unicode strings with their ascii equivalents
        var asciified = Emoji.Asciify("😉 :wink:");
        Assert.AreEqual(";) ;)", asciified);

        // replaces emoji shortcodes with raw unicode strings.
        var emojified = Emoji.Emojify("it's raining :cat:s and :dog:s!");
        Assert.AreEqual("it's raining 🐱s and 🐶s!", emojified);

        // replaces raw unicode strings with emoji shortcodes
        var demojified = Emoji.Demojify("it's raining 🐱s and 🐶s!");
        Assert.AreEqual("it's raining :cat:s and :dog:s!", demojified);

        // replaces emoji shortcodes and raw unicode strings with <img> tags
        var imagified = Emoji.Imagify("it's raining :cat:s and 🐶s!");
        Assert.AreEqual("""it's raining <img class="emoji" alt="🐱" title=":cat:" src="/emoji/1f431.png" />s and <img class="emoji" alt="🐶" title=":dog:" src="/emoji/1f436.png" />s!""", imagified);

        // replaces emoji shortcodes and raw unicode strings with <span> tags
        var spanified = Emoji.Spanify("it's raining :cat:s and 🐶s!");
        Assert.AreEqual("""it's raining <span class="emoji" title=":cat:">🐱</span>s and <span class="emoji" title=":dog:">🐶</span>s!""", spanified);

        // returns emoji with matching name, category, shortcodes or tags
        var emoji = Emoji.Find("smile").First();
        Assert.AreEqual("😃", emoji.Raw);

        // determines whether a string is comprised solely of emoji
        Assert.IsTrue(Emoji.IsEmoji("🐱🐶"));
        Assert.IsFalse(Emoji.IsEmoji("it's raining 🐱s and 🐶s!"));
    }

    [TestMethod]
    public void SymbolsAndDigitsAreNotEmoji() {
        var text = @" !""#$%&'()*+,-./0123456789:;<=>?@";
        var actual = Emoji.Demojify(text);
        Assert.AreEqual(text, actual);
    }

    [TestMethod]
    public void ToCodepoint() {
        // :grinning:
        var unicode = "😀";
        var codepoint = Emoji.ToCodePoint(unicode);
        Assert.AreEqual("1f600", codepoint);

        var actual = Emoji.FromCodePoint(codepoint);
        Assert.AreEqual(unicode, actual);

        var surrogate = Emoji.ToSurrogate(codepoint);
        Assert.AreEqual("\\ud83d\\ude00", surrogate);

        // unicode alternate
        unicode = "❤️";
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
        var emoji = Emoji.Get("🥶");
        Assert.AreEqual(":cold_face:", emoji.Shortcodes[0]);
    }

    [TestMethod]
    public void Version12Emoji() {
        var emoji = Emoji.Get("🥱");
        Assert.AreEqual(":yawning_face:", emoji.Shortcodes[0]);
    }

    [TestMethod]
    public void Version13Emoji() {
        Assert.AreEqual(117, Emoji.All.Count(e => e.Version == "13"));
        var emoji = Emoji.Get("🥲");
        Assert.AreEqual(":smiling_face_with_tear:", emoji.Shortcodes[0]);
    }

    [TestMethod]
    public void Version14Emoji() {
        Assert.AreEqual(112, Emoji.All.Count(e => e.Version == "14"));
        Assert.IsNotNull(Emoji.Get(":melting_face:"));
    }

    [TestMethod]
    public void Version15Emoji() {
        Assert.AreEqual(31, Emoji.All.Count(e => e.Version == "15"));
        Assert.IsNotNull(Emoji.Get(":shaking_face:"));
    }

    // Enable when emoji.json is pdated to 15.1
    //[TestMethod]
    //public void Version151Emoji() {
    //    Assert.IsNotNull(Emoji.Get(":lime:"));
    //    Assert.IsNotNull(Emoji.Get(":phoenix:"));
    //    Assert.AreEqual(118, Emoji.All.Count(e => e.Version == "15.1"));
    //}

    [TestMethod]
    public void ZWJSequence() {
        // :family: is a not a ZWJ sequence
        var raw = "👪";

        var codepoint = Emoji.ToCodePoint(raw);
        Assert.AreEqual("1f46a", codepoint);

        var shortcode = Emoji.Shortcode(raw);
        Assert.AreEqual(":family:", shortcode);

        // :family_mwgb: is a ZWJ sequence combining :man:, :woman:, :girl: and :boy:
        raw = "👨‍👩‍👧‍👦";
        codepoint = Emoji.ToCodePoint(raw);
        Assert.AreEqual("1f468-200d-1f469-200d-1f467-200d-1f466", codepoint);

        shortcode = Emoji.Shortcode(raw);
        Assert.AreNotEqual(":man:‍:woman:‍:girl:‍:boy:", shortcode);
        Assert.AreEqual(":family_mwgb:", shortcode);

        // :man_shrugging: is a ZWJ sequence combining :person_shrugging: (🤷), Zero Width Joiner (ZWJ) and :male_sign: (♂️)           
        raw = "🤷‍♂️";
        codepoint = Emoji.ToCodePoint(raw);
        Assert.AreEqual("1f937-200d-2642-fe0f", codepoint);

        shortcode = Emoji.Shortcode(raw);
        Assert.AreNotEqual(":person_shrugging:‍:male_sign:", shortcode);
        Assert.AreEqual(":man_shrugging:", shortcode);

        var surrogate = Emoji.ToSurrogate(codepoint);
        Assert.AreNotEqual(@"\ud83e\udd37", surrogate);
        Assert.AreEqual(@"\ud83e\udd37\u200d\u2642\ufe0f", surrogate);
    }
}
