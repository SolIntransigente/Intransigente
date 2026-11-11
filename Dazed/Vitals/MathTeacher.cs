using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Dazed.Vitals;

public static class MathTeacher
{
    public const float RadiansToDegrees = 180 / 3.1415927f;
    public const float DegreesToRadians = 3.1415927f / 180;

    /// <summary>
    /// Returns the equivalent of the base Quaternion into a Vector3 in degrees.
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static Vector3 ToVector3(this Quaternion q)
    {
        Vector3 angles;

        float sinX = 2 * (q.W * q.X + q.Y * q.Z);
        float cosX = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = MathF.Atan2(sinX, cosX);

        float sinY = 2 * (q.W * q.Y - q.Z * q.X);
        if (MathF.Abs(sinY) >= 1)
            angles.Y = MathF.CopySign(MathF.PI / 2, sinY);
        else
            angles.Y = MathF.Asin(sinY);

        float sinZ = 2 * (q.W * q.Z + q.X * q.Y);
        float cosZ = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = MathF.Atan2(sinZ, cosZ);


        angles.X = angles.X * RadiansToDegrees;
        angles.Y = angles.Y * RadiansToDegrees;
        angles.Z = angles.Z * RadiansToDegrees;

        return angles;
    }

    public static Quaternion ToQuaternion(this Vector3 v) => Quaternion.CreateFromYawPitchRoll(v.Y * DegreesToRadians, v.X * DegreesToRadians, v.Z * DegreesToRadians);

    public static Vector3 Up(this Matrix4x4 m) => new(m.M12, m.M22, m.M32);
    public static Vector3 Right(this Matrix4x4 m) => new(m.M11, m.M21, m.M31);
    public static Vector3 Forward(this Matrix4x4 m) => new(m.M13, m.M23, m.M33);
    
    /// <summary>
    /// If the integer value falls under 0 it is clamped to such number.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static uint GetSafeUnsignedInteger(this int value) => value < 0 ? 0 : (uint)value;

    public static int GetNextBiggerRoundedNumber(this float value)
    {
        int rounded = Convert.ToInt32(MathF.Round(value));
        return value < rounded ? rounded - 1 : rounded;
    }

    public static uint GetNextBiggerRoundedUnsignedNumber(this float value)
    {
        uint rounded = GetSafeUnsignedInteger((int)MathF.Round(value));
        return value < rounded ? rounded - 1 : rounded;
    }

    public static Vector3 GetMiddleWith(this Vector3 v1, Vector3 v2)
    {
        var x = (v1.X + v2.X) / 2;
        var y = (v1.Y + v2.Y) / 2;
        var z = (v1.Z + v2.Z) / 2;
        return new(x, y, z);
    }

    public static float Clamp(this float value, float a, float b)
    {
        if (value < a)
            value = a;
        if (value > b)
            value = b;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToRadians(float degrees) => degrees * DegreesToRadians;

    internal static Matrix4x4 ToNumerics(Assimp.Matrix4x4 t)
    {
        return new(t.A1, t.B1, t.C1, t.D1, t.A2, t.B2, t.C2, t.D2, t.A3, t.B3, t.C3, t.D3, t.A4, t.B4, t.C4, t.D4);
    }

    public static Matrix4 ToOpenTkMatrix4(Matrix4x4 data)
    {
        return new(data.M11, data.M12, data.M13, data.M14, 
            data.M21, data.M22, data.M23, data.M24,
            data.M31, data.M32, data.M33, data.M34,
            data.M41, data.M42, data.M43, data.M44);
    }

    public static bool Within(float min, float value, float max)
    {
        if (value < min)
            return false;
        return !(value > max);
    }

    internal static OpenTK.Mathematics.Vector3 ToOpenTK(Vector3 v) => new(v.X, v.Y, v.Z);
    internal static Vector3 ToNumerics(OpenTK.Mathematics.Vector3 v) => new(v.X, v.Y, v.Z);
}
