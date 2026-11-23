using System.Reflection;
using Dazed.Vitals.FileHandling;

namespace Dazed.Vitals.AssetReference
{
    public static class ExposedData
    {
        private readonly static Dictionary<string, object?> _data = [];
        public const BindingFlags ExposedFieldFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
        static ExposedData()
        {
            FindData();
        }

        private static void FindData()
        {
            _data.Clear();
            var path = PathUtils.BaseDirectoryCombine(CommonPaths.ExposedDataFile);
            if (!Path.Exists(path))
                if (DazedInstance.IsContextRuntime)
                    throw new Exception("No exposed data file found, make sure it hasn't been moved or deleted");
                else
                    Secretary.CreateNewFile(path, true);
            var cast = Cereal.LoadDataOrSetDefault<ExposedDataArrays>(path);
            if (cast.ClassAndField == null)
                return;
            for (int i = 0; i < cast.ClassAndField.Length; i++)
                _data.Add(cast.ClassAndField[i], cast.FieldData[i]);
        }

        public static bool TryRetrieve(string formalFieldName, out object obj, bool forceFindData = false)
        {
            if (forceFindData)
                FindData();
            obj = null!;
            try
            {
                obj = _data[formalFieldName]!;
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }

        public static bool RetrieveAndCast<T>(string formalFieldName, out T castedObject)
        {
            var val = TryRetrieve(formalFieldName, out var obj);
            if (val)
            {
                castedObject = (T)obj;
                return true;
            }
            else
                throw new NullReferenceException();
        }

        public static void RetrieveAll(Type thisType, object? thisInstance)
        {
            FieldInfo[] fields = thisType.GetFields(ExposedFieldFlags);
            foreach(FieldInfo field in fields)
            {
                Attribute? att = field.GetCustomAttribute<ExposedFieldAttribute>(true);
                if (att == null)
                    continue;
                string id = ConstructID(((ExposedFieldAttribute)att).FormalizedName, thisType.Name);
                var success = TryRetrieve(id, out var val);
                if (!success)
                    throw new NullReferenceException(field.Name);
                if (field.FieldType.GetInterface(nameof(IExposedElement)) != null)
                    field.SetValue(thisInstance, Activator.CreateInstance(field.FieldType, [val]));
                else
                    field.SetValue(thisInstance, val);
            }
        }

        public static string ConstructID(string fieldName, Type classType) => $"{classType.Name}|{fieldName}";
        public static string ConstructID(string fieldName, string className) => $"{className}|{fieldName}";
    }
}
