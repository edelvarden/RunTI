using System;
using System.Diagnostics;
using System.IO;

namespace RunTI
{
    internal class Destroy
    {
        public static void DestroyFileOrFolder(string path)
        {
            string formattedPath = path.Trim().TrimStart('"').TrimEnd('"');

            if (string.IsNullOrWhiteSpace(formattedPath) ||
                (!File.Exists(formattedPath) && !Directory.Exists(formattedPath)))
            {
                Console.WriteLine($"Invalid path: {formattedPath}");
                return;
            }

            try
            {
                // Take ownership and delete directory
                if (Directory.Exists(formattedPath))
                {
                    Takeown(formattedPath, true);
                    Icacls(formattedPath);
                    Directory.Delete(formattedPath, true);
                }
                // Take ownership and delete file
                else
                {
                    Takeown(formattedPath);
                    Icacls(formattedPath);
                    File.Delete(formattedPath);
                }
            }
            catch{}
        }

        private static void Takeown(string path, bool isDirectory = false)
        {
            string commandArgs = $"/f \"{path}\"";
            if (isDirectory)
            {
                commandArgs += " /r /d y";
            }

            try
            {
                RunCommand("takeown", commandArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing takeown: {ex.Message}");
            }
        }

        private static void Icacls(string path)
        {
            try
            {
                RunCommand("icacls", $"\"{path}\" /grant *S-1-5-32-544:F");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing icacls: {ex.Message}");
            }
        }

        private static void RunCommand(string fileName, string arguments)
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
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Command failed with exit code {process.ExitCode}: {error}");
                }
            }
        }
    }
}
