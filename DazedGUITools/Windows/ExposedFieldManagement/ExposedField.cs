using Dazed.Vitals.AssetReference;
using System;

namespace DazedGUITools.Windows.ExposedFieldManagement
{
    internal readonly struct ExposedField(string cN, string fF, Type fT, string? exEx)
    {
        public readonly string ClassName = cN;
        public readonly string FieldFormalName = fF;
        public readonly Type FieldType = fT;
        public readonly string? ExpectedExtension = exEx;
        public string FormattedName => ExposedData.ConstructID(FieldFormalName, ClassName);
    }
}
