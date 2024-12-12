using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantomLis.BoomboxExtended;

public static class FilePathDatabase
{
    private static Dictionary<string, string> db = new (); 

    public static void LoadDatabase(string db_file)
    {
        try
        {
            string[] f = File.ReadAllLines(db_file);
            string path, hash;
            foreach (var line in f)
            {
                path = line.Split(' ')[0];
                hash = line.Split(' ')[1];
                db.Add(hash,path);
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            _CreateNewFile(db_file);
        }
    }

    public static string[] GetHashes() => db.Keys.ToArray();
    public static string[] GetPaths() => db.Values.ToArray();
    public static string? GetPath(string hash) => db[hash];
    
    private static void _CreateNewFile(string dbFile)
    {
        throw new NotImplementedException();
    }
}