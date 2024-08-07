using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RunTI
{
    internal class Destroy
    {
        public static void DestroyFileOrFolder(string path)
        {
            string exe = "powershell.exe";
            string arguments = "-NoProfile -ExecutionPolicy Unrestricted";
            string formattedPath = path.Trim().TrimStart('"').TrimEnd('"');
            string formattedCommandPath = @"\" + $"\"{formattedPath}" + @"\" + "\""; // format path to \"path\"

            string group = @"\" + $"\"*S-1-5-32-544:F" + @"\" + "\""; // \"Everyone:(F)\"
            string everyone = @"\" + $"\"Everyone:(F):(OI)(CI)F" + @"\" + "\""; // \"Everyone:(F)\"
            string admins = @"\" + $"\"Administrators:(F):(OI)(CI)F" + @"\" + "\""; // \"Administrators:(F)\"
            string users = @"\" + $"\"Users:(F):(OI)(CI)F" + @"\" + "\""; // \"Users:(F)\"
            string system = @"\" + $"\"SYSTEM:(F):(OI)(CI)F" + @"\" + "\""; // \"SYSTEM:(F)\"
            string trustedinstaller = @"\" + $"\"NT SERVICE\\TrustedInstaller:(F):(OI)(CI)F" + @"\" + "\""; // \"NT SERVICE\TrustedInstaller:(F)\"

            string elevatePrivilagesCommand = $"takeown /f {formattedCommandPath};";

            if (IsDirectory($"{formattedPath}"))
            {
                elevatePrivilagesCommand = $"takeown /f {formattedCommandPath} /r /d y;";

                RunCmdScript($"takeown /f \"{formattedPath}\" /r /d y && icacls \"{formattedPath}\" /grant *S-1-5-32-544:F");
            }else
            {
                RunCmdScript($"takeown /f \"{formattedPath}\" && icacls \"{formattedPath}\" /grant *S-1-5-32-544:F");
            }

            elevatePrivilagesCommand +=
                $"icacls {formattedCommandPath} /setowner SYSTEM /t; " +
                $"icacls {formattedCommandPath} /grant {system} /t; " +
                $"icacls {formattedCommandPath} /grant {admins} /t; " +
                $"icacls {formattedCommandPath} /grant {users} /t; " +
                $"icacls {formattedCommandPath} /grant {everyone} /t; " +
                $"icacls {formattedCommandPath} /grant {group} /t";

            string deleteCommand = $"-Command \"{elevatePrivilagesCommand}; Remove-Item -Path {formattedCommandPath} -Recurse -Force -ErrorAction SilentlyContinue\"";

            arguments += " " + deleteCommand;

            if (Program.StartTiService())
            {
                string command = $"/SwitchTI /NoWindow /Dir:\"{Environment.CurrentDirectory.Replace(@"\", @"\\")}\" /Run:\"{exe}\" {arguments}";
                LegendaryTrustedInstaller.RunWithTokenOf("winlogon.exe", true, Application.ExecutablePath, command, false);
            }
        }

        private static void RunCmdScript(string script)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c {script}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }

        public static bool IsDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            FileAttributes attributes = File.GetAttributes(path);
            return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
