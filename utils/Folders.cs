using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick.Utils;

/// <summary>
/// Utility class for filesystem operations on Godot resource paths.
/// </summary>
public static class Folders
{
    /// <summary>
    /// Returns the file names (not full paths) of all non-directory entries directly inside
    /// <paramref name="path"/>. The scan is non-recursive: only immediate children are returned.
    /// </summary>
    /// <param name="path">
    /// An absolute Godot resource path to the directory to scan
    /// (e.g. <c>"res://entities/humans/human/animations"</c>).
    /// </param>
    /// <returns>A set of file names found in the directory.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="path"/> cannot be opened as a directory.
    /// </exception>
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