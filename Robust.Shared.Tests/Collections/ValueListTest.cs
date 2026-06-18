using System.Reflection;
using NUnit.Framework;
using Robust.Shared.Collections;

namespace Robust.Shared.Tests.Collections;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture, TestOf(typeof(ValueList<>))]
internal sealed class ValueListTest
{
    [Test]
    public void TryPopClearsRemovedReference()
    {
        var list = new ValueList<object>(1);
        var item = new object();
        list.Add(item);

        Assert.That(list.TryPop(out var popped), Is.True);
        Assert.That(popped, Is.SameAs(item));

        var itemsField = typeof(ValueList<object>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        var items = (object?[]?) itemsField!.GetValue(list);

        Assert.That(items, Is.Not.Null);
        Assert.That(items![0], Is.Null);
    }
}
