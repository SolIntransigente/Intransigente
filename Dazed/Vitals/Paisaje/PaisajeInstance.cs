using Dazed.Vitals.Content;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;

public class PaisajeInstance
{
    private readonly PaisajeObject[] m_objects;
    private readonly uint[] m_dynamicObjsIndices;
    public PaisajeVariables PaisajeVariables => m_vars;
    private PaisajeVariables m_vars;

    private readonly uint m_currentIndex;
    
    public PaisajeInstance(string assetAbsolutePath)
    {
        var sp = Cereal.LoadData<SerializedPaisaje>(assetAbsolutePath);
        m_objects = new PaisajeObject[sp.Elements.Length];
        var indicesToDynamics = new List<uint>();

        m_vars = sp.PaisajeVariables;
        for(uint i = 0; i < sp.Elements.Length; i++)
        {
            var elementType = GetElementType(sp.Elements[i]);
            var obj = PaisajeObject.ConstructObject(typeof(PaisajeObject), m_currentIndex++, sp.Elements[i]);
            m_objects[i] = obj;
            if (elementType != typeof(Gemet))
                indicesToDynamics.Add(i);
        }
        m_dynamicObjsIndices = [.. indicesToDynamics];
    }

    private static Type GetElementType(SerializedElementData element)
    {
        if (element.Dynamic == null)
            return typeof(Gemet);
        return Mundo.GetElementType(element.Dynamic.Value.TypeName);
    }

    public void Render(in RenderingData data)
    {
        //TODO: Implement sections and call those instead
        foreach (var obj in m_objects)
            obj.DrawUpdate(data);
    }

    internal void Destroy()
    {
        foreach(var index in m_dynamicObjsIndices)
           ((IDynamic)(m_objects[index])).Destroy();
    }
}