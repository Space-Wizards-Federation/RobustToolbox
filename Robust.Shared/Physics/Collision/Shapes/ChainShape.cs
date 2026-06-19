using System;
using System.Linq;
using System.Numerics;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Serialization;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Robust.Shared.Physics.Collision.Shapes;

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class ChainShape : IPhysShape
{
    [DataField]
    public Vector2[] Vertices = Array.Empty<Vector2>();

    public int Count => Vertices.Length - 1;

    private float _radius = PhysicsConstants.PolygonRadius;

    [DataField(customTypeSerializer: typeof(NonNegativeFloatSerializer))]
    public float Radius
    {
        get => _radius;
        set
        {
            if (value < 0f)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Chain radius cannot be negative.");

            _radius = value;
        }
    }

    [DataField]
    public Vector2 PrevVertex;

    [DataField]
    public Vector2 NextVertex;

    public void Clear()
    {
        Vertices = Array.Empty<Vector2>();
    }

    /// <summary>
    /// Creates a circular loop with the specified radius.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="outer">Does the chain block the outside (CCW) or inside (CW).</param>
    /// <param name="count">How many multiply radius by count to get total edges.</param>
    public void CreateLoop(Vector2 position, float radius, bool outer = true, float count = 16f)
    {
        if (radius < 0f)
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "Loop radius cannot be negative.");

        int divisions = Math.Max(16,(int)(radius * count));
        float arcLength = MathF.PI * 2 / divisions;
        Span<Vector2> vertices = stackalloc Vector2[divisions];

        for (int i = 0; i < divisions; i++)
        {
            var index = outer ? i : -i;
            var startPos = new Vector2(MathF.Cos(arcLength * index) * radius, MathF.Sin(arcLength * index) * radius);
            vertices[i] = startPos;
        }

        CreateLoop(vertices);
    }

    /// <summary>
    /// Creates a chain loop with the specified vertices and count.
    /// </summary>
    public void CreateLoop(ReadOnlySpan<Vector2> vertices)
    {
        var count = vertices.Length;
        DebugTools.Assert(Vertices.Length == 0);
        DebugTools.Assert(count >= 3);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (count < 3)
        {
            return;
        }

#if DEBUG
        for (var i = 1; i < count; ++i)
        {
            var v1 = vertices[i-1];
            var v2 = vertices[i];
            // If the code crashes here, it means your vertices are too close together.
            DebugTools.Assert((v1 - v2).LengthSquared() > PhysicsConstants.LinearSlop * PhysicsConstants.LinearSlop);
        }
#endif

        Array.Resize(ref Vertices, count + 1);
        vertices.CopyTo(Vertices);
        Vertices[count] = Vertices[0];
        PrevVertex = Vertices[Count - 2];
        NextVertex = Vertices[1];
    }

    public void CreateChain(ReadOnlySpan<Vector2> vertices, Vector2 prevVertex, Vector2 nextVertex)
    {
        var count = vertices.Length;
        DebugTools.Assert(Vertices.Length == 0);
        DebugTools.Assert(count >= 2);
#if DEBUG
        for (var i = 1; i < count; ++i)
        {
            // If the code crashes here, it means your vertices are too close together.
            DebugTools.Assert((vertices[i-1] - vertices[i]).LengthSquared() > PhysicsConstants.LinearSlop * PhysicsConstants.LinearSlop);
        }
#endif

        Array.Resize(ref Vertices, count);
        vertices.CopyTo(Vertices);

        PrevVertex = prevVertex;
        NextVertex = nextVertex;
    }

    public EdgeShape GetChildEdge(ref EdgeShape edge, int index)
    {
        DebugTools.Assert(0 <= index && index < Count - 1);
        // edge.Radius = Radius; (Let's be real we're never using this anyway).
        Vector2 vertex0;
        Vector2 vertex3;

        if (index > 0)
        {
            vertex0 = Vertices[index - 1];
        }
        else
        {
            vertex0 = PrevVertex;
        }

        if (index < Count - 2)
        {
            vertex3 = Vertices[index + 2];
        }
        else
        {
            vertex3 = NextVertex;
        }

        edge.SetOneSided(vertex0, Vertices[index + 0], Vertices[index + 1], vertex3);
        return edge;
    }

    public bool Equals(IPhysShape? other)
    {
        if (other is not ChainShape cShape)
            return false;

        return Equals(cShape);
    }

    public bool Equals(ChainShape otherChain)
    {
        return Count == otherChain.Count &&
               NextVertex == otherChain.NextVertex &&
               PrevVertex == otherChain.PrevVertex &&
               Vertices.SequenceEqual(otherChain.Vertices);
    }

}
