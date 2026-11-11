namespace DazedGUITools.Windows.LevelEditor.Representations
{
    /// <summary>
    /// Marks which vertex coordinates should this vertex reference (copy)
    /// </summary>
    /// <param name="x0">Should copy the X coordinates of first referenced vertex?</param>
    /// <param name="y0">Should copy the Y coordinates of first referenced vertex?</param>
    /// <param name="z0">Should copy the Z coordinates of first referenced vertex?</param>
    public struct VertexReferenceFlags(bool x0, bool y0, bool z0)
    {
        public bool useX0 = x0, useY0 = y0, useZ0 = z0;
    }
}
