using System;
using System.IO;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Various IO utility functions.
    /// </summary>
    public static class IOUtilities
    {
        /// <summary>
        /// If the given file path already exists, this returns the same file path with a unique integer appended to the end.
        /// Otherwise, this returns the original file path.
        /// </summary>
        /// <param name="filePath">Absolute file path to make unique</param>
        /// <returns>A unique file path which may either be the original file path or the original path with a similar name</returns>
        public static string MakeFilePathUnique(string filePath)
        {
            SmartLogger.Log(DebugFlags.IOUtilities, "original path: " + filePath);
            if (File.Exists(filePath))
            {
                string newFilePath = null;
                bool filePathIsUnique = false;
                string fileExtension = Path.GetExtension(filePath);
                filePath = Path.ChangeExtension(filePath, null);
                SmartLogger.Log(DebugFlags.IOUtilities, "original path extension: " + fileExtension);
                SmartLogger.Log(DebugFlags.IOUtilities, "original path without extension: " + filePath);
                for (ulong i = 0; i <= ulong.MaxValue; i++)
                {
                    newFilePath = filePath + "(" + i + ")" + fileExtension;
                    if (!File.Exists(newFilePath))
                    {
                        filePathIsUnique = true;
                        break;
                    }
                }
                if (filePathIsUnique)
                {
                    filePath = newFilePath;
                    SmartLogger.Log(DebugFlags.IOUtilities, "new path: " + newFilePath);
                }
                else
                {
                    return null;
                }
            }

            return filePath;
        }
    }
}
