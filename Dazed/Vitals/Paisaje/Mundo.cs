using System.Reflection;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;

public static class Mundo
{
    /// <summary>
    /// <p>Equivalent to a level in concept, holds the current "Environment data", this being geometry, entities and such</p>
    /// </summary>
    public static PaisajeInstance? CurrentPaisaje => m_pInstance;
    public static PaisajeVariables? Variables => CurrentPaisaje?.PaisajeVariables;
    private static PaisajeInstance? m_pInstance;

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
    internal static void DrawPaisaje(in RenderingData data) => CurrentPaisaje?.Render(data);

    internal static Type GetElementType(string identifier) => Elements[identifier];

    internal static void LoadPaisaje(AssetPath paisajeFilePath)
    {
        m_pInstance = new PaisajeInstance(paisajeFilePath.AssetRelativePath);
        Artist.AmbientLightIntensity = Variables!.Value.AmbientLightIntensity;
        Artist.AmbientLightColor = Variables.Value.AmbientLightColor;
    }
}