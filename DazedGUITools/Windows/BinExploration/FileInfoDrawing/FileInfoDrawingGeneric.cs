namespace DazedGUITools.Windows.BinExploration.FileInfoDrawing;

public abstract class FileInfoDrawingGeneric<T> : FileInfoDrawingBase where T : struct
{
    protected abstract T FileData {get; set;}
}