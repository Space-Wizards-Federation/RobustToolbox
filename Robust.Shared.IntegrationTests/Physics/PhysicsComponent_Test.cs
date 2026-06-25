using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using NUnit.Framework;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Robust.UnitTesting.Shared.Physics
{
    [TestFixture]
    [TestOf(typeof(PhysicsComponent))]
    internal sealed class PhysicsComponent_Test : RobustIntegrationTest
    {
        [Test]
        public async Task TestPointLinearImpulse()
        {
            var server = StartServer();
            await server.WaitIdleAsync();
            var entManager = server.ResolveDependency<IEntityManager>();
            var fixtureSystem = server.ResolveDependency<IEntitySystemManager>()
                .GetEntitySystem<FixtureSystem>();
            var physicsSystem = server.ResolveDependency<IEntitySystemManager>()
                .GetEntitySystem<SharedPhysicsSystem>();

            await server.WaitAssertion(() =>
            {
                entManager.System<SharedMapSystem>().CreateMap(out var mapId);
                var boxEnt = entManager.SpawnEntity(null, new MapCoordinates(Vector2.Zero, mapId));
                var box = entManager.AddComponent<PhysicsComponent>(boxEnt);
                var poly = new PolygonShape();
                poly.SetAsBox(0.5f, 0.5f);
                fixtureSystem.CreateFixture(boxEnt, "fix1", new Fixture(poly, 0, 0, false), body: box);
                physicsSystem.SetFixedRotation(boxEnt, false, body: box);
                physicsSystem.SetBodyType(boxEnt, BodyType.Dynamic, body: box);
                Assert.That(box.InvI, Is.GreaterThan(0f));

                // Check regular impulse works
                physicsSystem.ApplyLinearImpulse(boxEnt, new Vector2(0f, 1f), body: box);
                Assert.That(box.LinearVelocity.Length, Is.GreaterThan(0f));

                // Reset the box
                physicsSystem.SetLinearVelocity(boxEnt, Vector2.Zero, body: box);
                Assert.That(box.LinearVelocity.Length, Is.EqualTo(0f));
                Assert.That(box.AngularVelocity, Is.EqualTo(0f));

                // Check the angular impulse is applied from the point
                physicsSystem.ApplyLinearImpulse(boxEnt, new Vector2(0f, 1f), new Vector2(0.5f, 0f), body: box);
                Assert.That(box.LinearVelocity.Length, Is.GreaterThan(0f));
                Assert.That(box.AngularVelocity, Is.Not.EqualTo(0f));
            });
        }

        [Test]
        public async Task TestShapeRadiusValidation()
        {
            var server = StartServer(new ServerIntegrationOptions
            {
                FailureLogLevel = LogLevel.Fatal,
            });
            await server.WaitIdleAsync();

            var entManager = server.ResolveDependency<IEntityManager>();
            var fixtureSystem = server.ResolveDependency<IEntitySystemManager>()
                .GetEntitySystem<FixtureSystem>();
            var physicsSystem = server.ResolveDependency<IEntitySystemManager>()
                .GetEntitySystem<SharedPhysicsSystem>();

            await server.WaitAssertion(() =>
            {
                entManager.System<SharedMapSystem>().CreateMap(out var mapId);
                var uid = entManager.SpawnEntity(null, new MapCoordinates(Vector2.Zero, mapId));
                var body = entManager.AddComponent<PhysicsComponent>(uid);
                var shape = new PhysShapeCircle(0.5f);
                var fixture = new Fixture(shape, 0, 0, false);
                fixtureSystem.CreateFixture(uid, "fix1", fixture, body: body);
                var fixtures = entManager.GetComponent<FixturesComponent>(uid);
                var xform = entManager.GetComponent<TransformComponent>(uid);

                Assert.Throws<DebugAssertException>(() =>
                    physicsSystem.SetRadius((uid, fixtures, body, xform), "fix1", fixture, shape, -0.5f));

                Assert.That(shape.Radius, Is.EqualTo(0.5f));
            });
        }
    }
}
