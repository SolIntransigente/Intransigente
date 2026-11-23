using Newtonsoft.Json;

namespace Dazed.Vitals.AssetReference
{
    public struct AssetPath : IExposedElement
    {
        [JsonProperty("p")]
        public readonly string AssetRelativePath;

        public override int GetHashCode() => AssetRelativePath.GetHashCode();

        public const string NoPathAssigned = "NO PATH ASSIGNED";
        public AssetPath(){ AssetRelativePath = NoPathAssigned;}

        public AssetPath(string fieldName, Type classType)
        {
            string id = ExposedData.ConstructID(fieldName, classType);
            ExposedData.RetrieveAndCast(id, out AssetRelativePath);
        }
        [JsonConstructor]
        public AssetPath(string assetRelativePath) => AssetRelativePath = assetRelativePath;

        public static implicit operator string(AssetPath path) => path.AssetRelativePath;
    }
}
