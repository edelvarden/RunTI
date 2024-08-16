using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RunTI
{
    internal class Destroy
    {
        public static bool DestroyFileOrFolder(string path)
        {
            string formattedPath = path.Trim().TrimStart('"').TrimEnd('"');

            if (!IsValidPath(formattedPath))
            {
                return false;
            }

            if(!HandleOwnership(formattedPath, Directory.Exists(formattedPath)))
            {
                return false;
            }
            return RemoveItem(formattedPath);
        }

        private static bool IsValidPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && (File.Exists(path) || Directory.Exists(path));
        }

        private static bool HandleOwnership(string path, bool isDirectory)
        {
            if(!Takeown(path, isDirectory))
            {
                return false;
            }
            return Icacls(path);
        }

        private static bool Takeown(string path, bool isDirectory)
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

        private static bool RemoveItem(string path)
        {
            string commandArgs = $" -ExecutionPolicy Unrestricted -NoProfile -Command \"Remove-Item -Path '{path}' -Recurse -Force\"";
            return ExecuteCommand("powershell.exe", commandArgs);
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

                // Read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if(process.ExitCode != 0)
                {
                    // Display the output and errors
                    MessageBox.Show($"Command: {fileName} {arguments}\nOutput: {output}\nError: {error}");
                }

                return process.ExitCode == 0;
            }
        }
    }
}
