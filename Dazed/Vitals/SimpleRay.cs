using System.Numerics;

namespace Dazed.Vitals;

public readonly struct SimpleRay
{
    public readonly Vector3 RayStart;
    public readonly Vector3 RayEnd;

    public SimpleRay(Vector3 rayStart, Vector3 rayEnd)
    {
        RayStart = rayStart;
        RayEnd = rayEnd;
    }

    public override string ToString()
    {
        return base.ToString() + $" | With starting position of <{RayStart.ToString()}> and end position of <{RayEnd.ToString()}>";
    }
}