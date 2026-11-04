using System;
using System.Numerics;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    public class Transform
    {
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Forward { get; private set; }

        public Matrix4x4 World { get; private set; }
        public Matrix4x4 Local { get; private set; }

        public Transform? Parent
        {
            get => _parent;
            set
            {
                if (_parent != null)
                    _parent.RemoveChild(this);
                _parent = value;
                if (_parent != null)
                    _parent.AddChild(this);
                ReloadWorldMatrix();
            }
        }
        private Transform? _parent;
        private readonly List<Transform> _children = new();

        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private Vector3 _scale = Vector3.One;

        public event Action<Matrix4x4>? TransformationChange;

        public Vector3 LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                UpdateTranslation();
            }
        }
        public Vector3 Position
        {
            get => _parent == null ? _localPosition : _parent.Position + _localPosition;
            set
            {
                if (_parent == null)
                    _localPosition = value;
                else
                    _localPosition = value - _parent.Position;
                UpdateTranslation();
            }
        }

        public Quaternion Rotation
        {
            get => _parent == null ? _localRotation : _parent.Rotation * _localRotation;
            set
            {
                if(_parent == null)
                    _localRotation = value;
                else
                    _localRotation = Quaternion.Inverse(_parent.Rotation) * value;
                _localRotation = Quaternion.Normalize(_localRotation);
                UpdateRotation();
            }
        }
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                UpdateScale();
            }
        }
        private Matrix4x4 _scaleM, _rotationM, _translationM;

        private void ReloadWorldMatrix()
        {
            Local = _scaleM * _rotationM * _translationM;
            World = _parent == null ? Local : Local * _parent.World;
            Up = World.Up();
            Forward = World.Forward();
            Right = World.Right();

            TransformationChange?.Invoke(World);
            if (_children.Count == 0)
                return;
            foreach (Transform child in _children)
                child.ReloadWorldMatrix();
        }
        /// <summary>
        /// Moves on the global axis
        /// </summary>
        /// <param name="movement"></param>
        public void MoveGlobal(Vector3 movement, bool normalized = false)
        {
            movement = normalized ? Vector3.Normalize(movement) : movement;
            MoveInternal(movement);
        }
        /// <summary>
        /// Moves on the local axis
        /// </summary>
        /// <param name="movement"></param>
        public void MoveLocal(Vector3 movement, bool normalized = true)
        {
            Vector3 transformedMovement =
                movement.X * Right +
                movement.Y * Up +
                movement.Z * Forward;
            if (normalized)
                transformedMovement = Vector3.Normalize(transformedMovement);
            MoveInternal(transformedMovement);
        }

        private void MoveInternal(Vector3 move)
        {
            _localPosition += move;
            UpdateTranslation();
        }

        private void UpdateTranslation()
        {
            _translationM = Matrix4x4.CreateTranslation(_localPosition);
            ReloadWorldMatrix();
        }
        private void UpdateRotation()
        {
            var newQuat = Matrix4x4.CreateFromQuaternion(_localRotation);
            if(_rotationM == newQuat)
                return;
            _rotationM = newQuat;
            ReloadWorldMatrix();
        }
        private void UpdateScale()
        {
            _scaleM = Matrix4x4.CreateScale(_scale);
            ReloadWorldMatrix();
        }

        public void TranslateX(float amount, bool localAxis = true)
        {
            Vector3 xTranslation = localAxis ? amount * Right : amount * Vector3.UnitX;
            _localPosition += xTranslation;
            UpdateTranslation();
        }
        public void TranslateY(float amount, bool localAxis = true)
        {
            Vector3 yTranslation = localAxis ? amount * Up : amount * Vector3.UnitY;
            _localPosition += yTranslation;
            UpdateTranslation();
        }
        public void TranslateZ(float amount, bool localAxis = true)
        {
            Vector3 zTranslation = localAxis ? amount * Forward : amount * Vector3.UnitZ;
            _localPosition += zTranslation;
            UpdateTranslation();
        }

        public void Rotate(Quaternion rotation)
        {
            rotation = Quaternion.Normalize(rotation);
            Rotation *= rotation;
        }

        public void Rotate(Vector3 rotation)
        {
            rotation *= MathTeacher.DegreesToRadians;
            var rt = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation.Z) *
                     Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotation.Y) *
                     Quaternion.CreateFromAxisAngle(Vector3.UnitX, rotation.X);
            Rotation *= rt;
        }

        public void RemoveChild(Transform child)
        {
            _children.Remove(child);
        }
        public void AddChild(Transform child)
        {
            _children.Add(child);
        }

        public Transform()
        {
            _translationM = Matrix4x4.CreateTranslation(_localPosition);
            _localRotation = Quaternion.Identity;
            _rotationM = Matrix4x4.CreateFromQuaternion(_localRotation);
            _scaleM = Matrix4x4.CreateScale(_scale);
            ReloadWorldMatrix();
        }
    }
}
