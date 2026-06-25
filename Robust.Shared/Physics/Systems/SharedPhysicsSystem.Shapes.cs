using System;
using System.Numerics;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Shapes;
using Robust.Shared.Utility;

namespace Robust.Shared.Physics.Systems;

public abstract partial class SharedPhysicsSystem
{
    public void SetRadius<TShape>(
        Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent,
        string fixtureId,
        Fixture fixture,
        TShape shape,
        float radius)
        where TShape : IPhysShape
    {
        if (!ValidateRadius(ent, fixtureId, radius) ||
            MathHelper.CloseTo(shape.Radius, radius) ||
            !ResolveShapeEntity(ref ent))
        {
            return;
        }

        shape.Radius = radius;

        UpdateFixtureShape(ent, fixtureId, fixture);
    }

    #region Circle

    public void SetPositionRadius(
        Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent,
        string fixtureId,
        Fixture fixture,
        PhysShapeCircle shape,
        Vector2 position,
        float radius)
    {
        if (!ValidateRadius(ent, fixtureId, radius) ||
            MathHelper.CloseTo(shape.Radius, radius) && shape.Position.EqualsApprox(position) ||
            !ResolveShapeEntity(ref ent))
        {
            return;
        }

        shape.Position = position;
        shape.Radius = radius;

        UpdateFixtureShape(ent, fixtureId, fixture);
    }

    public void SetPosition(
        Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent,
        string fixtureId,
        Fixture fixture,
        PhysShapeCircle circle,
        Vector2 position)
    {
        if (circle.Position.EqualsApprox(position) || !ResolveShapeEntity(ref ent))
            return;

        circle.Position = position;

        UpdateFixtureShape(ent, fixtureId, fixture);
    }

    #endregion

    #region Edge

    public void SetVertices(
        Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent,
        string fixtureId,
        Fixture fixture,
        EdgeShape edge,
        Vector2 vertex0,
        Vector2 vertex1,
        Vector2 vertex2,
        Vector2 vertex3)
    {
        if (!ResolveShapeEntity(ref ent))
            return;

        edge.Vertex0 = vertex0;
        edge.Vertex1 = vertex1;
        edge.Vertex2 = vertex2;
        edge.Vertex3 = vertex3;

        UpdateFixtureShape(ent, fixtureId, fixture);
    }

    #endregion

    #region Polygon

    public void SetVertices(
        Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent,
        string fixtureId,
        Fixture fixture,
        PolygonShape poly,
        Vector2[] vertices)
    {
        if (!ResolveShapeEntity(ref ent))
            return;

        poly.Set(vertices, vertices.Length);

        UpdateFixtureShape(ent, fixtureId, fixture);
    }

    #endregion

    /// <summary>
    /// Increases or decreases all fixtures of an entity in size by a certain factor.
    /// </summary>
    public void ScaleFixtures(Entity<FixturesComponent?> ent, float factor)
    {
        if (!_fixturesQuery.Resolve(ent, ref ent.Comp))
            return;

        foreach (var (id, fixture) in ent.Comp.Fixtures)
        {
            ScaleFixture(ent, id, fixture, factor);
        }
    }

    public int GetChildCount<TShape>(TShape shape) where TShape : IPhysShape
    {
        return shape switch
        {
            ChainShape chain => chain.Count - 1,
            _ => 1,
        };
    }

    public Box2 ComputeAABB<TShape>(TShape shape, Transform transform, int childIndex) where TShape : IPhysShape
    {
        switch (shape)
        {
            case ChainShape chain:
            {
                DebugTools.Assert(childIndex < chain.Count);

                var i1 = childIndex;
                var i2 = childIndex + 1;
                if (i2 == chain.Count)
                    i2 = 0;

                var v1 = Physics.Transform.Mul(transform, chain.Vertices[i1]);
                var v2 = Physics.Transform.Mul(transform, chain.Vertices[i2]);

                var lower = Vector2.Min(v1, v2);
                var upper = Vector2.Max(v1, v2);
                var r = new Vector2(chain.Radius, chain.Radius);
                return new Box2(lower - r, upper + r);
            }
            case EdgeShape edge:
            {
                DebugTools.Assert(childIndex == 0);

                var v1 = Physics.Transform.Mul(transform, edge.Vertex1);
                var v2 = Physics.Transform.Mul(transform, edge.Vertex2);
                var lower = Vector2.Min(v1, v2);
                var upper = Vector2.Max(v1, v2);
                var radius = new Vector2(edge.Radius, edge.Radius);
                return new Box2(lower - radius, upper + radius);
            }
            case PhysShapeAabb aabb:
                return new Box2Rotated(aabb.LocalBounds.Translated(transform.Position), transform.Quaternion2D.Angle, transform.Position)
                    .CalcBoundingBox()
                    .Enlarged(aabb.Radius);
            case PhysShapeCircle circle:
            {
                DebugTools.Assert(childIndex == 0);

                var p = transform.Position + Physics.Transform.Mul(transform.Quaternion2D, circle.Position);
                return new Box2(p.X - circle.Radius, p.Y - circle.Radius, p.X + circle.Radius, p.Y + circle.Radius);
            }
            case PolygonShape poly:
            {
                DebugTools.Assert(childIndex == 0);
                return ComputePolygonAabb(poly.Vertices, poly.VertexCount, poly.Radius, transform);
            }
            case Polygon poly:
            {
                DebugTools.Assert(childIndex == 0);
                return ComputePolygonAabb(poly._vertices.AsSpan, poly.VertexCount, poly.Radius, transform);
            }
            case SlimPolygon slim:
            {
                DebugTools.Assert(childIndex == 0);
                return ComputePolygonAabb(slim._vertices.AsSpan, slim.VertexCount, slim.Radius, transform);
            }
            default:
                throw new NotImplementedException($"Cannot compute AABB for {shape.GetType()}.");
        }
    }

    private static Box2 ComputePolygonAabb(ReadOnlySpan<Vector2> vertices, int vertexCount, float radius, Transform transform)
    {
        DebugTools.Assert(vertexCount > 0);
        var lower = Physics.Transform.Mul(transform, vertices[0]);
        var upper = lower;

        for (var i = 1; i < vertexCount; ++i)
        {
            var v = Physics.Transform.Mul(transform, vertices[i]);
            lower = Vector2.Min(lower, v);
            upper = Vector2.Max(upper, v);
        }

        var r = new Vector2(radius, radius);
        return new Box2(lower - r, upper + r);
    }

    public void ScaleFixture(Entity<FixturesComponent?> ent, string fixtureId, Fixture fixture, float factor)
    {
        switch (fixture.Shape)
        {
            case EdgeShape edge:
                SetVertices((ent.Owner, ent.Comp, null, null),
                    fixtureId,
                    fixture,
                    edge,
                    edge.Vertex0 * factor,
                    edge.Vertex1 * factor,
                    edge.Vertex2 * factor,
                    edge.Vertex3 * factor);
                break;
            case PhysShapeCircle circle:
                SetPositionRadius((ent.Owner, ent.Comp, null, null), fixtureId, fixture, circle, circle.Position * factor, circle.Radius * factor);
                break;
            case PolygonShape poly:
                var verts = poly.Vertices;

                for (var i = 0; i < poly.VertexCount; i++)
                {
                    verts[i] *= factor;
                }

                SetVertices((ent.Owner, ent.Comp, null, null), fixtureId, fixture, poly, verts);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private bool ResolveShapeEntity(ref Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent)
    {
        return _fixturesQuery.Resolve(ent.Owner, ref ent.Comp1) &&
               PhysicsQuery.Resolve(ent.Owner, ref ent.Comp2) &&
               XformQuery.Resolve(ent.Owner, ref ent.Comp3);
    }

    private void UpdateFixtureShape(
        Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent,
        string fixtureId,
        Fixture fixture)
    {
        if (ent.Comp2!.CanCollide &&
            BroadphaseQuery.TryGetComponent(ent.Comp3!.Broadphase?.Uid, out var broadphase))
        {
            _lookup.DestroyProxies(ent.Owner, fixtureId, fixture, ent.Comp3, broadphase);
            _lookup.CreateProxies(ent.Owner, fixtureId, fixture, ent.Comp3, ent.Comp2);
        }

        _fixtures.FixtureUpdate(ent.Owner, manager: ent.Comp1!, body: ent.Comp2);
    }

    private bool ValidateRadius(Entity<FixturesComponent?, PhysicsComponent?, TransformComponent?> ent, string fixtureId, float radius)
    {
        if (radius >= 0f)
            return true;

        Log.Error($"Tried to set fixture {fixtureId} on {ToPrettyString(ent.Owner)} to negative radius {radius}.");
        DebugTools.Assert(radius >= 0f);
        return false;
    }
}
