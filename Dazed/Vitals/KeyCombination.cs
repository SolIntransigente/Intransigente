using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    public enum KeyCombinationType
    {
        And,
        Or,
    }
    public readonly struct KeyCombination
    {
        private readonly KeyCombinationType _kCT;
        private readonly Keys[] _k;

        public bool KeysDown()
        {
            int count = 0;
            foreach(Keys key in _k)
            {
                if (Input.IsKeyDown(key))
                    if (_kCT == KeyCombinationType.Or)
                        return true;
                    else if (_kCT == KeyCombinationType.And)
                        count++;
            }
            return count == _k.Length;
        }
        public bool KeysUp()
        {
            int count = 0;
            foreach (Keys key in _k)
            {
                if (Input.IsKeyUp(key))
                    if (_kCT == KeyCombinationType.Or)
                        return true;
                    else if (_kCT == KeyCombinationType.And)
                        count++;
            }
            return count == _k.Length;
        }
        /// <summary>
        /// <seealso cref="KeysReleased"/> function is not affected by the <seealso cref="KeyCombinationType"/> as it would be difficult
        /// to detect both keys being released at the same time.
        /// </summary>
        /// <returns></returns>
        public bool KeysReleased()
        {
            foreach (Keys key in _k)
            {
                if (Input.WasKeyReleased(key))
                    return true;
            }
            return false;
        }

        public bool FirstHoldSecondPressed()
        {
            if(_k.Length != 2) return false;
            return Input.IsKeyDown(_k[0]) && Input.KeyJustPressed(_k[1]);
        }

        public KeyCombination(Keys[] keys, KeyCombinationType keyCombination = KeyCombinationType.And)
        {
            if (keys.Length == 1)
                Scriptorium.Warn("Key combination structure should not be used for single key triggers");
            _kCT = keyCombination;
            _k = keys;
        }

        public KeyCombination(Keys key0, Keys key1, KeyCombinationType keyCombination = KeyCombinationType.And)
        {
            _kCT = keyCombination;
            _k = [key0, key1];
        }
    }
}
