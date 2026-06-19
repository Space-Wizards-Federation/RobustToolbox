using System;
using Robust.Shared.Physics.Components;

namespace Robust.Shared.Physics.Collision.Shapes
{
    /// <summary>
    /// A primitive physical shape that is used by a <see cref="PhysicsComponent"/>.
    /// </summary>
    [NotContentImplementable]
    public interface IPhysShape : IEquatable<IPhysShape>
    {
        /// <summary>
        /// Radius of the Shape
        /// Changing the radius causes a recalculation of shape properties.
        /// </summary>
        float Radius { get; set; }

        // Sloth: I removed density because mass is way easier to work with.
        // If you really want it back then code it yaself (and also probably put it on the fixture).
    }
}
