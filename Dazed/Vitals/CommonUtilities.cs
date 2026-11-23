using System.Collections;
using System.Numerics;
using System.Reflection;
using Dazed.Vitals.AssetReference;

namespace Dazed.Vitals
{
    public static class CommonUtilities
    {
        private static readonly Dictionary<Type, object> CommonDefaults = [];

        static CommonUtilities()
        {
            CommonDefaults.Add(typeof(string), string.Empty);
            CommonDefaults.Add(typeof(int), (int)0);
            CommonDefaults.Add(typeof(uint), (uint)0);
            CommonDefaults.Add(typeof(short), (short)0);
            CommonDefaults.Add(typeof(ushort), (ushort)0);
            CommonDefaults.Add(typeof(float), (float)0);
            CommonDefaults.Add(typeof(Half), (Half)0);
            CommonDefaults.Add(typeof(Vector2), Vector2.Zero);
            CommonDefaults.Add(typeof(Vector3), Vector3.Zero);
            CommonDefaults.Add(typeof(Vector4), Vector4.Zero);
            CommonDefaults.Add(typeof(AssetPath), AssetPath.NoPathAssigned);
        }

        public static Type[] FindAllOfSubType(Type derivesFrom)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(x => x.GetExportedTypes());
            List<Type> selectedTypes = new();
            foreach (var type in types)
                if (!type.IsAbstract && !type.IsInterface && type.IsSubclassOf(derivesFrom))
                    selectedTypes.Add(type);
            return selectedTypes.ToArray();
        }
        public static Type[] FindAllOfInterfaceType(Type derivesFrom)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(x => x.GetExportedTypes());
            List<Type> selectedTypes = new();
            foreach (var type in types)
                if (!type.IsAbstract && type.GetInterface(derivesFrom.Name) != null)
                    selectedTypes.Add(type);
            return selectedTypes.ToArray();
        }

        public static Vector3 GetDirection(this Transform transform, ObjectDirection direction)
        {
            switch (direction)
            {
                case ObjectDirection.Right:
                    return transform.Right;
                case ObjectDirection.Left:
                    return -transform.Right;
                case ObjectDirection.Up:
                    return transform.Up;
                case ObjectDirection.Down:
                    return (-transform.Up);
                case ObjectDirection.Forward:
                    return transform.Forward;
                case ObjectDirection.Backward:
                    return (-transform.Forward);
            }
            return default;
        }
        public static Vector3 GetOppositeDirection(this Transform transform, ObjectDirection direction)
        {
            switch (direction)
            {
                case ObjectDirection.Right:
                    return -transform.Right;
                case ObjectDirection.Left:
                    return transform.Right;
                case ObjectDirection.Up:
                    return -transform.Up;
                case ObjectDirection.Down:
                    return transform.Up;
                case ObjectDirection.Forward:
                    return -transform.Forward;
                case ObjectDirection.Backward:
                    return transform.Forward;
            }
            return default;
        }

        /// <summary>
        /// Returns the default value of a common struct (float, string, int, uint, short, ushort)
        /// </summary>
        /// <param name="type">The type to evaluate</param>
        /// <returns>The default value for the struct</returns>
        public static object GetDefaultOfStruct(Type type)
        {
            if (type.IsClass || type.IsInterface)
                throw new ArgumentException("Type specified for getting its default must not be a class nor an interface, but a struct");
            // ! If type is not included, then warn (throw exception)
            return CommonDefaults[type];
        }

        public static bool IsOfInterface(object evaluated, Type type) => evaluated.GetType().GetInterface(type.Name) != null;
    }
}
