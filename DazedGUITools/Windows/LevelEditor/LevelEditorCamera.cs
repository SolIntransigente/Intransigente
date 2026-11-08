using Dazed.Vitals;
using System.Numerics;

namespace DazedGUITools.Windows.LevelEditor
{
    internal static class LevelEditorCamera
    {
        public static Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateWorld();
            }
        }
        private static Vector3 _position = Vector3.Zero;
        public static Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateWorld();
            }
        }
        private static Quaternion _rotation = Quaternion.Identity;
        internal static Matrix4x4 World { get; private set; } = Matrix4x4.Identity;
        internal static Matrix4x4 View { get; private set; } = Matrix4x4.Identity;
        internal static Matrix4x4 Projection { get; private set; } = Matrix4x4.Identity;

        internal static Vector3 Forward { get; private set; }
        internal static Vector3 Up { get; private set; }
        internal static Vector3 Right { get; private set; }

        internal static float FieldOfView { get; private set; }

        private static float s_fov, s_near;
        private static ushort s_far;

        public static void Initialize(float fieldOfViewDegrees = 90, float near = 0.4f, ushort far = 1000)
        {
            s_fov = MathTeacher.ToRadians(fieldOfViewDegrees);
            FieldOfView = s_fov;
            s_near = near;
            s_far = far;

            UpdateWorld();
        }

        private static void UpdateWorld()
        {
            World = Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_position);
            Forward = World.Forward();
            Up = World.Up();
            Right = -World.Right();
            UpdateProjection();
            View = Matrix4x4.CreateLookAt(_position, Forward + _position, Up);
        }

        public static void UpdateProjection() => Projection = Matrix4x4.CreatePerspectiveFieldOfView(s_fov, LevelEditorViewport.AspectRatio, s_near, s_far);

        public static void SetEulerRotation(Vector3 rotation)
        {
            rotation *= MathTeacher.DegreesToRadians;
            var rt = Quaternion.CreateFromAxisAngle(Vector3.UnitX, rotation.X) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotation.Y);
            _rotation = rt;
            _rotation = Quaternion.Normalize(_rotation);
            UpdateWorld();
        }

        internal static void AddPositionRelative(Vector3 value, float factor = 1)
        {
            Vector3 val = value.X * Right + value.Y * Up + value.Z * Forward;
            val = Vector3.Normalize(val);
            Position += val * factor;
        }
        internal static void Rotate(Vector3 rotation)
        {
            //Done this way, otherwise the rotation can affect other axis
            //If <X> and <Y> are modified, <Z> axis might also be modified, which is unwanted for the camera
            var rotX = new Vector3(rotation.X, 0, 0);
            var rotY = new Vector3(0, rotation.Y, 0);
            var qRtX = rotX.ToQuaternion();
            var qRtY = rotY.ToQuaternion();
            
            _rotation = qRtX * _rotation * qRtY;
            _rotation = Quaternion.Normalize(_rotation);
            UpdateWorld();
        }

        internal static void AddPosition(Vector3 value, float factor = 1) => Position += value * factor;
    }
}
