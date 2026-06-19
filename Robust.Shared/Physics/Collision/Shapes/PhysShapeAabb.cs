using System;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Serialization;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Robust.Shared.Physics.Collision.Shapes
{
    /// <summary>
    /// A physics shape that represents an Axis-Aligned Bounding Box.
    /// This box does not rotate with the entity, and will always be offset from the
    /// entity origin in world space.
    /// </summary>
    [Serializable, NetSerializable]
    [DataDefinition]
    public sealed partial class PhysShapeAabb : IPhysShape, IEquatable<PhysShapeAabb>
    {
        /// <summary>
        /// The radius of this AABB
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField(customTypeSerializer: typeof(NonNegativeFloatSerializer))]
        public float Radius
        {
            get => _radius;
            set
            {
                if (value < 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "AABB radius cannot be negative.");

                if (MathHelper.CloseToPercent(_radius, value)) return;
                _radius = value;
            }
        }

        private float _radius;

        internal Vector2 Centroid => Vector2.Zero;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        private Box2 _localBounds = Box2.UnitCentered;

        /// <inheritdoc />
        public Box2 LocalBounds => _localBounds;

        public PhysShapeAabb(float radius)
        {
            Radius = radius;
        }

        public PhysShapeAabb()
        {
            _radius = PhysicsConstants.PolygonRadius;
        }

        {
        }

        [Pure]
        internal List<Vector2> GetVertices()
        {
            return new()
            {
                _localBounds.BottomRight,
                _localBounds.TopRight,
                _localBounds.TopLeft,
                _localBounds.BottomLeft,
            };
        }

        public bool Equals(IPhysShape? other)
        {
            if (other is not PhysShapeAabb otherAABB) return false;
            return _localBounds.EqualsApprox(otherAABB._localBounds);
        }

        public bool Equals(PhysShapeAabb? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _radius.Equals(other._radius) && _localBounds.Equals(other._localBounds);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is PhysShapeAabb other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_radius, _localBounds);
        }
    }
}
