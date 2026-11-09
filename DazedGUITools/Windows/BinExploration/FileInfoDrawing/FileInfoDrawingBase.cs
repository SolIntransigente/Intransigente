namespace DazedGUITools.Windows.BinExploration.FileInfoDrawing;

public abstract class FileInfoDrawingBase
{
    public abstract string FilePath {get; protected set;}
    public abstract void Draw();
    public abstract void Initialize(string filePath);
}