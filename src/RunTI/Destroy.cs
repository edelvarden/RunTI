using System;
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

            string admins = @"\" + $"\"Administrators:(F)" + @"\" + "\""; // \"Administrators:(F)\"
            string users = @"\" + $"\"Users:(F)" + @"\" + "\""; // \"Users:(F)\"
            string system = @"\" + $"\"SYSTEM:(F)" + @"\" + "\""; // \"SYSTEM:(F)\"
            string trustedinstaller = @"\" + $"\"NT SERVICE\\TrustedInstaller:(F)" + @"\" + "\""; // \"NT SERVICE\TrustedInstaller:(F)\"

            string elevatePrivilagesCommand = $"takeown /f {formattedCommandPath}; icacls {formattedCommandPath} /grant {system} /t; icacls {formattedCommandPath} /grant {admins} /t; icacls {formattedCommandPath} /grant {users} /t";

            if (IsDirectory($"{formattedPath}"))
            {
                elevatePrivilagesCommand = $"takeown /f {formattedCommandPath} /r /d y; icacls {formattedCommandPath} /grant {system} /t; icacls {formattedCommandPath} /grant {admins} /t; icacls {formattedCommandPath} /grant {users} /t";
            }

            string deleteCommand = $"-Command \"{elevatePrivilagesCommand}; Remove-Item -Path {formattedCommandPath} -Recurse -Force -ErrorAction SilentlyContinue\"";

            arguments += " " + deleteCommand;

            if (Program.StartTiService())
            {
                string command = $"/SwitchTI /NoWindow /Dir:\"{Environment.CurrentDirectory.Replace(@"\", @"\\")}\" /Run:\"{exe}\" {arguments}";
                LegendaryTrustedInstaller.RunWithTokenOf("winlogon.exe", true, Application.ExecutablePath, command, false);
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
