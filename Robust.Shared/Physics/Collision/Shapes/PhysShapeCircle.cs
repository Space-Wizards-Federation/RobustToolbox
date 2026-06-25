using System;
using System.Numerics;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Serialization;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Robust.Shared.Physics.Collision.Shapes
{
    /// <summary>
    /// A physics shape that represents a circle. The circle cannot be rotated,
    /// and it's origin is always the same as the entity position.
    /// </summary>
    [Serializable, NetSerializable]
    [DataDefinition]
    public sealed partial class PhysShapeCircle : IPhysShape, IEquatable<PhysShapeCircle>
    {
        private const float DefaultRadius = 0.5f;

        private float _radius = DefaultRadius;

        [DataField(customTypeSerializer: typeof(NonNegativeFloatSerializer)),
         Access(typeof(SharedPhysicsSystem), Friend = AccessPermissions.ReadWriteExecute, Other = AccessPermissions.Read)]
        public float Radius
        {
            get => _radius;
            set
            {
                if (value < 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Circle radius cannot be negative.");

                _radius = value;
            }
        }

        [DataField, Access(typeof(SharedPhysicsSystem), Friend = AccessPermissions.ReadWriteExecute, Other = AccessPermissions.Read)]
        public Vector2 Position;

        public float Area => MathF.PI * _radius * _radius;

        public PhysShapeCircle()
        {
        }

        public PhysShapeCircle(float radius)
        {
            Radius = radius;
            Position = Vector2.Zero;
        }

        public PhysShapeCircle(float radius, Vector2 position)
        {
            Radius = radius;
            Position = position;
        }

        public Box2 CalcLocalBounds()
        {
            // circle inscribed in box
            return new Box2(
                Position.X - _radius,
                Position.Y - _radius,
                Position.X + _radius,
                Position.Y + _radius);
        }

        public bool Equals(IPhysShape? other)
        {
            if (other is not PhysShapeCircle otherCircle) return false;
            return otherCircle.Equals(this);
        }

        public bool Equals(PhysShapeCircle? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return MathHelper.CloseTo(_radius, other._radius) && Position.EqualsApprox(other.Position);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is PhysShapeCircle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_radius, Position);
        }
    }
}
