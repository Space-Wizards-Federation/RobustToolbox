using System.Numerics;
using System.Text;
using NUnit.Framework;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Robust.Client.Tests.UserInterface;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[TestOf(typeof(RichTextEntry))]
public sealed class RichTextEntryTest
{
    /// <summary>
    /// Asserts that a constant line doesn't have more spacing than it should.
    /// </summary>
    [Test]
    public void ConsistentLineSpacingUsesDefault()
    {
        var entry = CreateEntry("x\nx");
        var font = new TestFont();

        entry.Update(new MarkupTagManager(), font, 100, 1, consistentLineSpacing: true);

        Assert.Multiple(() =>
        {
            Assert.That(entry.LineAscents, Has.Count.EqualTo(2));
            Assert.That(entry.LineDescents, Has.Count.EqualTo(2));
            Assert.That(entry.LineAscents[0], Is.EqualTo(font.GetAscent(1)));
            Assert.That(entry.LineDescents[0], Is.EqualTo(font.GetDescent(1)));
            Assert.That(entry.Height, Is.EqualTo(font.GetHeight(1) + font.GetLineHeight(1)));
        });
    }

    /// <summary>
    /// Asserts that a line with inconsistent spacing expands and doesn't overlap.
    /// </summary>
    [Test]
    public void ConsistentLineSpacingExpandsOnlyOverlap()
    {
        var entry = CreateEntry("L\nx");
        var font = new TestFont();

        entry.Update(new MarkupTagManager(), font, 100, 1, consistentLineSpacing: true);

        Assert.Multiple(() =>
        {
            Assert.That(entry.LineAscents, Has.Count.EqualTo(2));
            Assert.That(entry.LineDescents, Has.Count.EqualTo(2));
            Assert.That(entry.LineAscents[0], Is.EqualTo(TestFont.LargeGlyph.BearingY));
            Assert.That(entry.LineDescents[0], Is.EqualTo(TestFont.LargeGlyph.Height - TestFont.LargeGlyph.BearingY));
            Assert.That(entry.LineAscents[1], Is.EqualTo(font.GetAscent(1)));
            Assert.That(entry.LineDescents[1], Is.EqualTo(font.GetDescent(1)));
            Assert.That(entry.Height, Is.EqualTo(40));
        });
    }

    /// <summary>
    /// Asserts we ignore the data if we only care about the font itself for line spacing.
    /// </summary>
    [Test]
    public void IgnoreLineDataForNonConsistentSpacing()
    {
        var entry = CreateEntry("L\nx");
        var font = new TestFont();

        entry.Update(new MarkupTagManager(), font, 100, 1);

        Assert.Multiple(() =>
        {
            Assert.That(entry.LineAscents, Is.Empty);
            Assert.That(entry.LineDescents, Is.Empty);
            Assert.That(entry.Height, Is.EqualTo(font.GetHeight(1) + font.GetLineHeight(1)));
        });
    }

    private static RichTextEntry CreateEntry(string text)
    {
        return new RichTextEntry(
            FormattedMessage.FromUnformatted(text),
            null!,
            new MarkupTagManager());
    }

    private sealed class TestFont : Font
    {
        public static readonly CharMetrics LargeGlyph = new(0, 24, 5, 5, 28);
        private static readonly CharMetrics NormalGlyph = new(0, 10, 5, 5, 12);

        public override int GetAscent(float scale) => 10;
        public override int GetHeight(float scale) => 12;
        public override int GetDescent(float scale) => 2;
        public override int GetLineHeight(float scale) => 14;

        public override float DrawChar(
            DrawingHandleBase handle,
            Rune rune,
            Vector2 baseline,
            float scale,
            Color color,
            bool fallback = true)
        {
            return GetCharMetrics(rune, scale, fallback)?.Advance ?? 0;
        }

        public override CharMetrics? GetCharMetrics(Rune rune, float scale, bool fallback = true)
        {
            return rune.Value == 'L' ? LargeGlyph : NormalGlyph;
        }
    }
}
