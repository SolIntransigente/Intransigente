using System.Reflection;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;

public static class Mundo
{
    /// <summary>
    /// <p>Equivalent to a level in concept, holds the current "Environment data", this being geometry, entities and such</p>
    /// </summary>
    public static PaisajeInstance? CurrentPaisaje
    {
        get;
        private set
        {
            field = value;
            ReloadPaisajeContext();
        }
    }
    public static PaisajeVariables? Variables
    {
        get => CurrentPaisaje?.Variables;
        private set
        {
            if (value == null)
                return;
            CurrentPaisaje?.Variables = value.Value;
        }
    }

    private static readonly Dictionary<string, Type> Elements = [];


    static Mundo()
    {
        //Get all paisaje elements, then store them for later.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> types = [];
        List<PaisajeElementAttribute> attributes = [];
        foreach (var t in assemblies)
        {
            var elements = GetPaisajeElements(t, out var atts);
            types.AddRange(elements);
            attributes.AddRange(atts);
        }

        for (var i = 0; i < types.Count; i++)
            Elements.Add(attributes[i].ElementIdentifier, types[i]);
    }

    private static List<Type> GetPaisajeElements(Assembly assembly, out List<PaisajeElementAttribute> attributes)
    {
        attributes = [];
        List<Type> elements = [];
        foreach (var type in assembly.GetTypes())
        {
            object? att = type.GetCustomAttribute<PaisajeElementAttribute>(true);
            if (att == null) continue;
            
            attributes.Add((PaisajeElementAttribute)att);
            elements.Add(type);
        }
        return elements;
    }
    private static void ReloadPaisajeContext()
    {
    }

    internal static void DrawPaisaje(Frustum frustum)
    {
    }
    
    internal static Type GetElementType(string identifier) => Elements[identifier];

    internal static void RetrieveGeometryData(SerializedPaisajeElement sb, out Half[] vertices, out ushort[] indices)
    {
        PaisajeGeometryData data = PaisajeGeometryData.RetrieveGeometryData(sb.ElementData);
        vertices = data.Vertices;
        indices = data.Indices;
    }

    internal static void LoadPaisaje(AssetPath paisajeFilePath)
    {
        var data = Cereal.LoadData<SerializedPaisaje>(PathUtils.BaseDirectoryCombine(paisajeFilePath.AssetRelativePath));
        //TODO: Process paisaje data and instantiate appropriate objects
    }

    internal static void CreateGeometry(Half[] vertices, ushort[] indices, out object[] elementData)
    {
        elementData = PaisajeGeometryData.OrganizeGeometryData(new PaisajeGeometryData(vertices, indices));
    }
}