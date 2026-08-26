using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick.Utils;

/// <summary>
/// // TODO: document this.
/// </summary>
public static class FolderUtils
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public static IReadOnlyCollection<string> ListFiles(string path)
    {
        var result = new HashSet<string>();

        using var dir = DirAccess.Open(path)
            ?? throw new InvalidOperationException($"Could not open directory: {path}");

        dir.ListDirBegin();
        var fileName = dir.GetNext();

        while (fileName != string.Empty)
        {
            if (!dir.CurrentIsDir())
                result.Add(fileName);

            fileName = dir.GetNext();
        }

        dir.ListDirEnd();

        return result;
    }
}