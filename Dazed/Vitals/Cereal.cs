using Newtonsoft.Json;

namespace Dazed.Vitals;

public static class Cereal
{
    public static bool SaveData<T>(T data, string absolutePath)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            if(!File.Exists(absolutePath))
                using (var f = File.Create(absolutePath)){}
            if (DebugState.DebugFull)
                Scriptorium.Warn($"File saved at path: {absolutePath} | Of type: {typeof(T)}");
            File.WriteAllText(absolutePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Scriptorium.LogException(ex);
#if !DEBUG
            throw ex;
#else
            return false;
#endif
        }
    }

    public static T LoadData<T>(string path)
    {
        var json = File.ReadAllText(path);
        if(DebugState.DebugFull)
            Scriptorium.Warn($"File loaded at path: {path} | Of type: {typeof(T)}");
        return JsonConvert.DeserializeObject<T>(json) ?? throw new InvalidOperationException();
    }
}