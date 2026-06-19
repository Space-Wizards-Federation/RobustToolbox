using NUnit.Framework;
using Robust.Shared.Physics.Serialization;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Markdown.Value;

namespace Robust.UnitTesting.Shared.Serialization.TypeSerializers;

[TestFixture]
[TestOf(typeof(NonNegativeFloatSerializer))]
internal sealed class NonNegativeFloatSerializerTest : OurSerializationTest
{
    [Test]
    public void ValidationAllowsZero()
    {
        var validation = Serialization.ValidateNode<float, ValueDataNode, NonNegativeFloatSerializer>(new ValueDataNode("0"));

        Assert.That(validation.GetErrors(), Is.Empty);
    }

    [Test]
    public void ValidationRejectsNegative()
    {
        var validation = Serialization.ValidateNode<float, ValueDataNode, NonNegativeFloatSerializer>(new ValueDataNode("-0.1"));

        Assert.That(validation.GetErrors(), Is.Not.Empty);
    }

    [Test]
    public void ReadRejectsNegative()
    {
        Assert.That(
            () => Serialization.Read<float, ValueDataNode, NonNegativeFloatSerializer>(new ValueDataNode("-0.1")),
            Throws.InstanceOf<InvalidMappingException>());
    }
}
