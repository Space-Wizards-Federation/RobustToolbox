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
    public Box2Rotated Box = new(Box2.UnitCentered.Translated(new Vector2(1, 2)), Angle.FromDegrees(37), new Vector2(1, 2));
    public Matrix3x2 Matrix = Matrix3x2.CreateScale(1.5f, 0.5f) * Matrix3x2.CreateRotation(0.5f) * Matrix3x2.CreateTranslation(3, -1);
    private Matrix3x2 _matrixResult;
    private Box2 _boxResult;

    [Benchmark]
    public void GetTransform()
    {
        _matrixResult = Box.Transform;
    }

    [Benchmark(Baseline = true)]
    public void TransformBoxOld()
    {
        _boxResult = (Box.Transform * Matrix).TransformBox(Box.Box);
    }

    [Benchmark]
    public void TransformBox()
    {
        _boxResult = Matrix.TransformBox(Box);
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
