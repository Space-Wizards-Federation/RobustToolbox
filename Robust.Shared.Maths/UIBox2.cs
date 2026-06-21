using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Robust.Shared.Utility;

namespace Robust.Shared.Maths
{
    /// <summary>
    ///     Axis Aligned rectangular box in screen coordinates.
    ///     Uses a left-handed coordinate system. This means that X+ is to the right and Y+ down.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct UIBox2 : IEquatable<UIBox2>, ISpanFormattable
    {
        /// <summary>
        ///     The X coordinate of the left edge of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 0)] internal float _left;

        /// <summary>
        ///     The Y coordinate of the top edge of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 1)] internal float _top;

        /// <summary>
        ///     The X coordinate of the right edge of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 2)] internal float _right;

        /// <summary>
        ///     The Y coordinate of the bottom of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 3)] internal float _bottom;

        [FieldOffset(sizeof(float) * 0)] internal Vector2 _topLeft;
        [FieldOffset(sizeof(float) * 2)] internal Vector2 _bottomRight;

        /// <summary>
        ///     The X coordinate of the left edge of the box.
        /// </summary>
        public float Left
        {
            readonly get => _left;
            set
            {
                if (value > _right)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Left cannot be greater than Right.");

                _left = value;
            }
        }

        /// <summary>
        ///     The Y coordinate of the top edge of the box.
        /// </summary>
        public float Top
        {
            readonly get => _top;
            set
            {
                if (value > _bottom)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Top cannot be greater than Bottom.");

                _top = value;
            }
        }

        /// <summary>
        ///     The X coordinate of the right edge of the box.
        /// </summary>
        public float Right
        {
            readonly get => _right;
            set
            {
                if (value < _left)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Right cannot be less than Left.");

                _right = value;
            }
        }

        /// <summary>
        ///     The Y coordinate of the bottom of the box.
        /// </summary>
        public float Bottom
        {
            readonly get => _bottom;
            set
            {
                if (value < _top)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Bottom cannot be less than Top.");

                _bottom = value;
            }
        }

        public Vector2 TopLeft
        {
            readonly get => _topLeft;
            set
            {
                if (value.X > _right)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "TopLeft.X cannot be greater than Right.");

                if (value.Y > _bottom)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "TopLeft.Y cannot be greater than Bottom.");

                _topLeft = value;
            }
        }

        public Vector2 BottomRight
        {
            readonly get => _bottomRight;
            set
            {
                if (value.X < _left)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "BottomRight.X cannot be less than Left.");

                if (value.Y < _top)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "BottomRight.Y cannot be less than Top.");

                _bottomRight = value;
            }
        }

        public readonly Vector2 TopRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Right, Top);
        }

        public readonly Vector2 BottomLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Left, Bottom);
        }

        public readonly float Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _right - _left;
        }

        public readonly float Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bottom - _top;
        }

        public readonly Vector2 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Width, Height);
        }

        public readonly Vector2 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Vector2(_left + _right, _top + _bottom) / 2f;
        }

        private static void Validate(float left, float top, float right, float bottom)
        {
            if (left > right)
                throw new ArgumentException("Left cannot be greater than Right.", nameof(left));

            if (top > bottom)
                throw new ArgumentException("Top cannot be greater than Bottom.", nameof(top));
        }

        public UIBox2(Vector2 leftTop, Vector2 rightBottom)
        {
            Unsafe.SkipInit(out this);

            Validate(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);

            _topLeft = leftTop;
            _bottomRight = rightBottom;
        }

        public UIBox2(float left, float top, float right, float bottom)
        {
            Unsafe.SkipInit(out this);

            Validate(left, top, right, bottom);

            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }

        /// <summary>
        /// Creates a UIBox2 with no bounds validation applied, use at your own risk.
        /// </summary>
        internal static UIBox2 DangerousCreate(float left, float top, float right, float bottom)
        {
            Unsafe.SkipInit(out UIBox2 box);
            box._left = left;
            box._right = right;
            box._top = top;
            box._bottom = bottom;
            return box;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static UIBox2 FromDimensions(float left, float top, float width, float height)
        {
            return new(left, top, left + width, top + height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static UIBox2 FromDimensions(Vector2 leftTopPosition, Vector2 size)
        {
            return FromDimensions(leftTopPosition.X, leftTopPosition.Y, size.X, size.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly bool Intersects(UIBox2 other)
        {
            return other._bottom >= _top
                   && other._top <= _bottom
                   && other._right >= _left
                   && other._left <= _right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly bool IsEmpty()
        {
            return MathHelper.CloseToPercent(Width, 0.0f) && MathHelper.CloseToPercent(Height, 0.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly bool Encloses(UIBox2 inner)
        {
            return Left < inner.Left && Bottom > inner.Bottom && Right > inner.Right && Top < inner.Top;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly bool Contains(in UIBox2 inner)
            => Left <= inner.Left
               && Top <= inner.Top
               && Right >= inner.Right
               && Bottom >= inner.Bottom;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly bool Contains(float x, float y)
        {
            return Contains(new Vector2(x, y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly bool Contains(Vector2 point, bool closedRegion = true)
        {
            var xOk = closedRegion
                ? point.X >= Left ^ point.X > Right
                : point.X > Left ^ point.X >= Right;
            var yOk = closedRegion
                ? point.Y >= Top ^ point.Y > Bottom
                : point.Y > Top ^ point.Y >= Bottom;
            return xOk && yOk;
        }

        /// <summary>
        ///     Uniformly scales the box by a given scalar.
        ///     This scaling is done such that the center of the resulting box is the same as this box.
        ///     i.e. it scales around the center of the box, just changing width/height.
        /// </summary>
        /// <param name="scalar">Value to scale the box by.</param>
        /// <returns>Scaled box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly UIBox2 Scale(float scalar)
        {
            if (scalar < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scalar), scalar, "Scalar cannot be negative.");
            }

            var center = Center;
            var halfSize = Size / 2 * scalar;
            return new UIBox2(
                center - halfSize,
                center + halfSize);
        }

        /// <summary>Returns a UIBox2 translated by the given amount.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public readonly UIBox2 Translated(Vector2 point)
        {
            return new(Left + point.X, Top + point.Y, Right + point.X, Bottom + point.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UIBox2 other)
        {
            return Left.Equals(other.Left) && Right.Equals(other.Right) && Top.Equals(other.Top) &&
                   Bottom.Equals(other.Bottom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object? obj)
        {
            if (obj is null) return false;
            return obj is UIBox2 box2 && Equals(box2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left.GetHashCode();
                hashCode = (hashCode * 397) ^ Right.GetHashCode();
                hashCode = (hashCode * 397) ^ Top.GetHashCode();
                hashCode = (hashCode * 397) ^ Bottom.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        ///     Compares two objects for equality by value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UIBox2 a, UIBox2 b)
        {
            return MathHelper.CloseToPercent(a.Bottom, b.Bottom) &&
                   MathHelper.CloseToPercent(a.Right, b.Right) &&
                   MathHelper.CloseToPercent(a.Top, b.Top) &&
                   MathHelper.CloseToPercent(a.Left, b.Left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UIBox2 a, UIBox2 b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UIBox2 operator +(UIBox2 box, (float lo, float to, float ro, float bo) offsets)
        {
            var (lo, to, ro, bo) = offsets;

            return new UIBox2(box.Left + lo, box.Top + to, box.Right + ro, box.Bottom + bo);
        }

        public override readonly string ToString()
        {
            return $"({Left}, {Top}, {Right}, {Bottom})";
        }

        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            return ToString();
        }

        public readonly bool TryFormat(
            Span<char> destination,
            out int charsWritten,
            ReadOnlySpan<char> format,
            IFormatProvider? provider)
        {
            return FormatHelpers.TryFormatInto(
                destination,
                out charsWritten,
                $"({Left}, {Top}, {Right}, {Bottom})");
        }
    }
}
