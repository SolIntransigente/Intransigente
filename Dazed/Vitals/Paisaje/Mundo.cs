using System.Reflection;
using Dazed.Vitals.Paisaje.Entities;
using Dazed.Vitals.Rendering;
using System.Numerics;

namespace Dazed.Vitals.Paisaje;

public static class Mundo
{
    public static PaisajeInstance? CurrentPaisaje
    {
        get;
        set
        {
            field = value;
            ReloadPaisajeContext();
        }
    }
    public static PaisajeVariables? Variables
    {
        get => CurrentPaisaje?.Variables;
        set
        {
            if (value == null)
                return;
            CurrentPaisaje?.Variables = value.Value;
        }
    }

    private static readonly Dictionary<string, Type> Elements = new();


    static Mundo()
    {
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

    private static List<Type> GetPaisajeElements(Assembly assembly, out List<PaisajeElementAttribute> atts)
    {
        atts = [];
        List<Type> elements = [];
        foreach (var type in assembly.GetTypes())
        {
            object? att = type.GetCustomAttribute<PaisajeElementAttribute>(true);
            if (att == null) continue;
            
            atts.Add((PaisajeElementAttribute)att);
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
    
    public static Type GetElementType(string identifier) => Elements[identifier];

    public static void RetrieveGeometryData(SerializedPaisajeElement sb, out float[] vertices, out uint[] indices)
    {
        PaisajeGeometryData data = PaisajeGeometryData.RetrieveGeometryData(sb.ElementData);
        vertices = data.Vertices;
        indices = data.Indices;
    }

    public static void CreateGeometry(float[] vertices, uint[] indices, out object[] elementData)
    {
        elementData = PaisajeGeometryData.OrganizeGeometryData(new PaisajeGeometryData(vertices, indices));
    }
}