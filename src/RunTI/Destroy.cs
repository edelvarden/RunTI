using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RunTI
{
    internal class Destroy
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetFileAttributes(string lpFileName, FileAttributes dwFileAttributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool RemoveDirectory(string lpPathName);

        public static void DestroyFileOrFolder(string path)
        {
            string formattedPath = Path.GetFullPath(path).Trim().TrimStart('"').TrimEnd('"');

            if (!IsValidPath(formattedPath))
            {
                return;
            }

            if (Directory.Exists(formattedPath))
            {
                HandleOwnership(formattedPath, true);
                DeleteDirectory(formattedPath);
            }
            else
            {
                HandleOwnership(formattedPath);
                DeleteSingleFile(formattedPath);
            }
        }

        private static bool IsValidPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && (File.Exists(path) || Directory.Exists(path));
        }

        private static void HandleOwnership(string path, bool isDirectory = false)
        {
            Takeown(path, isDirectory);
            Icacls(path);
        }

        private static void DeleteSingleFile(string path)
        {
            SetFileAttributes(path, FileAttributes.Normal);
            DeleteFile(path);
        }

        private static void DeleteDirectory(string path)
        {
            SetFileAttributes(path, FileAttributes.Normal);

            foreach (var file in Directory.GetFiles(path))
            {
                DeleteSingleFile(file);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                DeleteDirectory(dir);
            }

            RemoveDirectory(path);
        }

        private static bool Takeown(string path, bool isDirectory = false)
        {
            string commandArgs = $"/f \"{path}\"";
            if (isDirectory)
            {
                commandArgs += " /r /d y";
            }

            return ExecuteCommand("takeown", commandArgs);
        }

        private static bool Icacls(string path)
        {
            return ExecuteCommand("icacls", $"\"{path}\" /grant *S-1-5-32-544:F");
        }

        private static bool ExecuteCommand(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = psi })
            {
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
    }
}
