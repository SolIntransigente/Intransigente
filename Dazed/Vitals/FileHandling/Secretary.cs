using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals.FileHandling
{
    /// <summary>
    /// Provides safe IO operations, catching errors and preventing them
    /// </summary>
    public static class Secretary
    {
        /// <summary>
        /// </summary>
        /// <param name="folder">The path of the directory / folder</param>
        /// <param name="file">The file name and extension</param>
        /// <param name="forceFolderCreation">Whether a folder should be created if the path provided doesn't exist</param>
        /// <returns>
        /// The content of the file, if it exists. If the file or the folder doesn't exists (or an unexpected exception was thrown) then a null string is returned.
        /// </returns>
        public static string? ReadContent(string folder, string file, bool forceFolderCreation = true, bool filePathIsAbsolute = false)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(file))
            {
                Scriptorium.Warn($"Either folder <{folder}> or file <{file}> path is empty / null");
                return null;
            }
            try
            {
                if (!CanUseFolder(folder, forceFolderCreation))
                    return null;
                var filePath = filePathIsAbsolute ? file : Path.Combine(folder, file);
                if (!CanUseFile(false, filePath))
                    return null;
                return File.ReadAllText(filePath);
            }
            catch(Exception ex)
            {
                Scriptorium.LogException(ex);
            }
            return null;
        }
        public static void SaveContent(string folder, string file, string data, out bool successful, bool forceFolderCreation = true, bool forceFileCreation = true)
        {
            successful = false;
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(file))
            {
                Scriptorium.Warn($"Either folder <{folder}> or file <{file}> path is empty / null");
                return;
            }
            try
            {
                if (!CanUseFolder(folder, forceFolderCreation))
                    return;
                var filePath = Path.Combine(folder, file);
                if (!CanUseFile(forceFileCreation, filePath))
                    return;
                File.WriteAllText(filePath, data);
                successful = true;
            }
            catch(Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public static void SaveContent(string path, string data, out bool successful, bool forceFolderCreation = true, bool forceFileCreation = true)
        {
            successful = false;
            if (string.IsNullOrEmpty(path))
            {
                Scriptorium.Warn($"<{path}> path is empty / null");
                return;
            }
            try
            {
                if (!CanUseFolder(path, forceFolderCreation))
                    return;
                if (!CanUseFile(forceFileCreation, path))
                    return;
                File.WriteAllText(path, data);
                successful = true;
            }
            catch(Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public static bool SaveContent(string path, string data, bool forceFolderCreation = true, bool forceFileCreation = true)
        {
            SaveContent(path, data, out bool wasSuccessful, forceFolderCreation, forceFileCreation);
            return wasSuccessful;
        }
        public static bool SaveContent(string folder, string file, string data, bool forceFolderCreation = true, bool forceFileCreation = true)
        {
            SaveContent(folder, file, data, out bool wasSuccessful, forceFolderCreation, forceFileCreation);
            return wasSuccessful;
        }

        private static bool CanUseFile(bool forceFileCreation, string filePath)
        {
            if (!File.Exists(filePath))
                if (forceFileCreation)
                {
                    var f = File.Create(filePath);
                    f.Dispose();
                }
                else
                {
                    if (DebugState.DebugLow) Scriptorium.Warn($"File <{filePath}> can not be used / created, as it doesn't exist and won't be created by command");
                    return false;
                }
            return true;
        }

        private static bool CanUseFolder(string folder, bool forceFolderCreation)
        {
            //Checking if folder path is actually a file path
            if (folder.Contains('.'))
                folder = Path.GetPathRoot(folder);
            if (!Directory.Exists(folder))
                if (forceFolderCreation)
                    Directory.CreateDirectory(folder);
                else
                {
                    if (DebugState.DebugLow) Scriptorium.Warn($"Content could not be saved or loaded, as the folder <{folder}> doesn't exists and is not forced to be created");
                    return false;
                }

            return true;
        }
    }
}
