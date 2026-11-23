namespace Dazed.Vitals.AssetReference
{
    /// <summary>
    /// Exposes the field to the editor and allows a value to be assigned.<br></br>
    /// It is pseudo static and readonly, as every instance of the object will be assigned the same value at creation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExposedFieldAttribute : Attribute
    {
        public readonly string FormalizedName;
        public readonly string? ExpectedExtension;
        public ExposedFieldAttribute(string name, string expectedExtension)
        {
            FormalizedName = name;
            ExpectedExtension = expectedExtension;
        }
        public ExposedFieldAttribute(string name)
        {
            FormalizedName = name;
        }
    }
}
