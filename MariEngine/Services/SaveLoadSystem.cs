using System;
using System.IO;
using MariEngine.Persistence;

namespace MariEngine.Services;

public class SaveLoadSystem(string saveDirectory) : Service
{
    private const string IndexFileName = "index.yml";
    
    public SaveLoadContext LoadSaveFile(string saveName)
    {
        return new SaveLoadContext(Path.Combine(saveDirectory, saveName, IndexFileName));
    }
}