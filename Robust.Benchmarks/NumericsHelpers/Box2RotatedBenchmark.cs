using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using Robust.Shared.Analyzers;
using Robust.Shared.Maths;

namespace Robust.Benchmarks.NumericsHelpers;

[Virtual, DisassemblyDiagnoser]
public class Box2RotatedBenchmark
{
    public Box2Rotated Box = new();

    [Benchmark]
    public Matrix3x2 GetTransform()
    {
        return Box.Transform;
    }
}

[Virtual, DisassemblyDiagnoser]
public class Box2RotatedBoundingBoxBenchmark
{
    public Box2Rotated ZeroRotationBox = new(new Box2(-1, -2, 3, 4), Angle.Zero, new Vector2(0.25f, -0.5f));
    public Box2Rotated RotatedBox = new(new Box2(-1, -2, 3, 4), 0.7f, new Vector2(0.25f, -0.5f));

    private Box2 _boxResult;

    [Benchmark(Baseline = true)]
    public void CurrentZeroRotation()
    {
        _boxResult = ZeroRotationBox.CalcBoundingBox();
    }

    [Benchmark]
    public void NoCheckZeroRotation()
    {
        _boxResult = CalcBoundingBoxNoRotationCheck(ZeroRotationBox);
    }

    [Benchmark]
    public void CurrentRotated()
    {
        _boxResult = RotatedBox.CalcBoundingBox();
    }

    [Benchmark]
    public void NoCheckRotated()
    {
        _boxResult = CalcBoundingBoxNoRotationCheck(RotatedBox);
    }

    private static Box2 CalcBoundingBoxNoRotationCheck(in Box2Rotated box)
    {
        box.GetVertices(out var x, out var y);
        var aabb = SimdHelpers.GetAABB(x, y);
        return Unsafe.As<Vector128<float>, Box2>(ref aabb);
    }
}
