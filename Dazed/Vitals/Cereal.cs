using Dazed.Vitals.FileHandling;
using Newtonsoft.Json;

namespace Dazed.Vitals;

public static class Cereal
{
    public static bool SaveData<T>(T data, string folder, string file)
    {
        var jsonContent = JsonConvert.SerializeObject(data);
        return Secretary.SaveContent(folder, file, jsonContent);
    }

    /// <summary>
    /// Finds and reads the file present in the path, then returns it casted into class (T)
    /// </summary>
    /// <param name="path">The path to the file (Relative or absolute)</param>
    /// <returns>The casted, instantiated, object</returns>
    public static T LoadData<T>(string path)
    {
        var json = File.ReadAllText(path);
        if(DebugState.DebugFull)
            Scriptorium.Warn($"File loaded at path: {path} | Of type: {typeof(T)}");
        //! Don't worry if it is null, it should trigger an exception if so
        return JsonConvert.DeserializeObject<T>(json)!;
    }

    /// <summary>
    /// Safely* loads the file present in the path set, then tries to return the object casted into (T)
    /// <br>*Safely, as it catches exceptions logs them in the console and then returns successful as true
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <param name="successful">Whether it succeeded in the process or not</param>
    /// <returns></returns>
    public static T? SafeLoadData<T>(string path, out bool successful)
    {
        successful = false;
        try
        {
            T val = LoadData<T>(path);
            successful = true;
            return val;
        }
        catch(Exception ex)
        {
            Scriptorium.Error($"The file at path <{path}> failed to be loaded and casted");
            Scriptorium.LogException(ex);
            return default;
        }
    }
}