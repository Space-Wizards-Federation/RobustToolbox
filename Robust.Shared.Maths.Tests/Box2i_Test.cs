using NUnit.Framework;
﻿using System;
using NUnit.Framework;

namespace Robust.Shared.Maths.Tests
{
    [TestFixture, Parallelizable, TestOf(typeof(Box2i))]
    internal sealed class Box2i_Test
    {
        [Test]
        public void Box2iUnion()
        {
            var boxOne = new Box2i(-1, -1, 1, 1);
            var boxTwo = new Box2i(0, 0, 2, 2);

            var result = boxOne.Union(boxTwo);

            Assert.That(result.Left, Is.EqualTo(-1));
            Assert.That(result.Bottom, Is.EqualTo(-1));
            Assert.That(result.Right, Is.EqualTo(2));
            Assert.That(result.Top, Is.EqualTo(2));
        }

        [Test]
        public void Box2iVector2iUnion()
        {
            var box = new Box2i();
            Assert.That(box, Is.EqualTo(Box2i.Empty));

            box = box.UnionTile(Vector2i.Zero);
            Assert.That(box.Right, Is.EqualTo(1));

            box = box.UnionTile(Vector2i.One);
            Assert.That(box.Top, Is.EqualTo(2));

            box = box.Union(new Vector2i(2, 0));
            Assert.That(box.Right, Is.EqualTo(2));
        }

        [Test]
        public void Box2iUsesDirectDimensions()
        {
            var valid = new Box2i(-1, -2, 3, 4);

            Assert.Multiple(() =>
            {
                Assert.That(valid.Width, Is.EqualTo(4));
                Assert.That(valid.Height, Is.EqualTo(6));
                Assert.That(valid.Size, Is.EqualTo(new Vector2i(4, 6)));
                Assert.That(valid.IsValid(), Is.True);
            });
        }

        [Test]
        public void Box2iValidatesConstruction()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => new Box2i(3, 4, -1, -2));
                Assert.Throws<ArgumentException>(() => new Box2i(new Vector2i(3, 4), new Vector2i(-1, -2)));
            });
        }

        [Test]
        public void Box2iValidatesProperties()
        {
            var box = new Box2i(-1, -2, 3, 4);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => box.Left = 4);
                Assert.Throws<ArgumentOutOfRangeException>(() => box.Bottom = 5);
                Assert.Throws<ArgumentOutOfRangeException>(() => box.Right = -2);
                Assert.Throws<ArgumentOutOfRangeException>(() => box.Top = -3);
                Assert.Throws<ArgumentOutOfRangeException>(() => box.BottomLeft = new Vector2i(4, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => box.TopRight = new Vector2i(0, -3));
            });
        }
    }
}
