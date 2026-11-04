namespace Dazed.Vitals.Paisaje;

public readonly struct PaisajeGeometryData(Half[] vertices, ushort[] indices)
{
    public readonly Half[] Vertices = vertices;
    public readonly ushort[] Indices = indices;

    internal static object[] OrganizeGeometryData(PaisajeGeometryData d)
    {
        object[] all = new object[d.Indices.Length + d.Vertices.Length];
        for (int v = 0; v < d.Vertices.Length; v++)
            all[v] = d.Vertices[v];
        for(int i = 0; i < d.Indices.Length; i++)
            all[i + d.Vertices.Length - 1] = d.Indices[i];
        return all;
    }

    internal static PaisajeGeometryData RetrieveGeometryData(ElementData Data)
    {
        var all = Data.Data;
        List<Half> vertices = [.. all.TakeWhile(v => v is not ushort).Cast<Half>()];
        
        var indices = new ushort[all.Length - vertices.Count];
        for (var i = 0; i < indices.Length; i++)
            indices[i] = (ushort)all[i + vertices.Count - 1];
        return new PaisajeGeometryData(vertices.ToArray(), indices);
    }
}