using System.Numerics;

namespace DazedGUITools.Windows.LevelEditor.Representations;

internal interface IGeometryRepresentation : ILESelectableObject
{
    public string MaterialPath { get; set; }
    public bool UseZToYTextureCoordinate { get; set; }
    public bool UseZToXTextureCoordinate { get; set; }
    public bool FlipTextureCoordinateX { get; set; }
    public bool FlipTextureCoordinateY { get; set; }

    public void SetPosition(int selectedVertexIndex, Vector3 vertPos);
    public void SwapZParenting();
    public bool TrySetMaterial(string m_tempPath, out string message);
    public VertexData VertexAt(int selectedVertexIndex);
}