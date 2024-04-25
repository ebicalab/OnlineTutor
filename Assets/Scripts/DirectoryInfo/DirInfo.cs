using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DirInfo
{
    static public int getCountOfFilesInFolder(string path, string extension = ".mp3")
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + path);
        int count = 0;
        foreach (var file in dirInfo.GetFiles())
        {
            if (file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }
        return count;
    }

    static public int getCountOfFiles(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + path);
        return dirInfo.GetFiles().Length;
    }

    static public int getCountOfFolders(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + path);
        return dirInfo.GetDirectories().Length;
    }
}
