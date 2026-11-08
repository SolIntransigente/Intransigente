namespace DazedGUITools.Windows.LevelEditor.Representations;

internal class SectionRepresentation
{
    public ushort SectionUniqueID {get; set;}
    public bool Visible {get; set;} = true;
    

    public SectionRepresentation(ushort uniqueID)
    {
        SectionUniqueID = uniqueID;
    }
}