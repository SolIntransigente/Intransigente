using Dazed.Vitals.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    public static class Input
    {
        private static KeyboardState _kState = null!;
        private static MouseState _mState = null!;

        public static Vector2 PreviousMousePosition => _previousMousePosition;
        private static Vector2 _previousMousePosition;

        public static Vector2 RelativeMousePosition => _relMousePos;
        private static Vector2 _relMousePos;
        public static Vector2 MousePosition => _mousePosition;
        private static Vector2 _mousePosition;

        public static Vector2 MouseDelta => _mouseDelta;
        private static Vector2 _mouseDelta;

        public static void UpdateInputState(KeyboardState keyboardState,  MouseState mouseState)
        {
            _kState = keyboardState;
            _mState = mouseState;
            _mousePosition = _mState.Position;
            float x = MousePosition.X * Artist.NormalizedWidth;
            float y = MousePosition.Y * Artist.NormalizedHeight;
            if (x > 1)
                x = 1;
            else if (x < 0)
                x = 0;
            if (y > 1)
                y = 1;
            else if (y < 0)
                y = 0;
            _relMousePos = new(x, y);
            _previousMousePosition = _mState.PreviousPosition;
            _mouseDelta = _mState.Delta;
        }

        public static bool IsKeyDown(Keys key) => _kState.IsKeyDown(key);
        public static bool IsKeyDown(MouseButton button) => _mState.IsButtonDown(button);
        public static bool LeftClickDown() => _mState.IsButtonDown(MouseButton.Left);
        public static bool RightClickDown() => _mState.IsButtonDown(MouseButton.Right);
        public static bool MiddleClickDown() => _mState.IsButtonDown(MouseButton.Middle);


        public static bool IsKeyUp(Keys key) => !_kState.IsKeyDown(key);
        public static bool IsKeyUp(MouseButton button) => !_mState.IsButtonDown(button);

        public static bool WasKeyReleased(Keys key) => _kState.IsKeyReleased(key);
        public static bool WasKeyReleased(MouseButton button) => _mState.IsButtonReleased(button);
        
        public static bool KeyJustPressed(Keys key) => _kState.IsKeyPressed(key);
        public static bool KeyJustPressed(MouseButton button) => _mState.IsButtonPressed(button);
    }
}
