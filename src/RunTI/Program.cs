using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RunTI
{
    static class Program
    {
        // Prevent idiots to touch this executable
        static readonly FileStream thisExec = new FileStream(Application.ExecutablePath, FileMode.Open, FileAccess.Read);

        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments, for more information use /help");
                return;
            }

            switch (args[0].ToLower())
            {
                case "/help":
                    DisplayHelpMessage();
                    return;
                case "/disablenetwork":
                    ToggleAdapters(false);
                    return;
                case "/enablenetwork":
                    ToggleAdapters(true);
                    return;
                case "/destroyfileorfolder":
                    DestroyFileOrFolder(string.Join(" ", args.Skip(1)));
                    return;
                default:
                    break;
            }
                
            void ToggleAdapters(bool isEnabled = true)
            {
                ToggleAdapter("Ethernet", isEnabled);
                ToggleAdapter("Ethernet0", isEnabled);
            }

            void ToggleAdapter(string interfaceName, bool isEnabled)
            {
                string status = isEnabled ? "enable" : "disable";
                try
                {
                    System.Diagnostics.ProcessStartInfo psi =
                       new System.Diagnostics.ProcessStartInfo
                       {
                           FileName = "netsh",
                           Arguments = String.Format("interface set interface \"{0}\" {1}", interfaceName, status),
                           UseShellExecute = false,
                           CreateNoWindow = true,
                           RedirectStandardOutput = true,
                       };
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo = psi;
                    p.Start();
                }
                catch { }
                
            }

#if UACB
            if (!(args.Length > 0))
            {
                Bypass();
                return;
            }
            else
            {
                //This exposes to 34987567834598345 bugs so don't use UACSkip pls
                List<string> b = new List<string>(args);
                var pos = b.IndexOf("/SwitchTI");
                if (pos == -1)
                    args = new string[0];
            }
#endif

            if (args.Length > 0 && args[0].ToLower() == "/switchti")
                ParseCmdLine(args);
            else
                LaunchWithParams(args);
        }

#if UACB
        static void Bypass()
        {
            Registry.CurrentUser.OpenSubKey("Environment", true).SetValue("windir", $"{Application.ExecutablePath} /bypass ", RegistryValueKind.String);

            var si = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                UseShellExecute = false,
                Arguments = @"/RUN /TN Microsoft\Windows\DiskCleanup\SilentCleanup /I"
            };

            var p = Process.Start(si);
            p.WaitForExit();

            Registry.CurrentUser.OpenSubKey("Environment", true).DeleteValue("windir");
        }
#endif

        static void DisplayHelpMessage()
        {
            Console.WriteLine("");
            Console.WriteLine("/EXEFilename\t\tName of the executable.");
            Console.WriteLine("/CommandLine\t\tArguments for the executable.");
            Console.WriteLine("/StartDirectory\t\tDirectory where the executable starts.");
            Console.WriteLine("/NoWindow\t\tStart the executable without any windows (useful for silent script running).");
            Console.WriteLine("");
            Console.WriteLine("/DisableNetwork\t\tDisable network connection.");
            Console.WriteLine("/EnableNetwork\t\tEnable network connection.");
            Console.WriteLine("");
            Console.WriteLine("/DestroyFileOrFolder\tDestroy file or folder without confirmation and without moving to recycle bin.");
        }

        static void LaunchWithParams(string[] args)
        {
            string exe = "cmd.exe";
            string arguments = "";
            string dirPath = "";
            bool isWindow = true;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/exefilename":
                        exe = args[++i];
                        break;
                    case "/commandline":
                        arguments = args[++i];
                        break;
                    case "/startdirectory":
                        dirPath = args[++i];
                        break;
                    case "/nowindow":
                        isWindow = false;
                        break;
                    default:
                        Console.WriteLine($"Unknown argument: {args[i]}");
                        return;
                }
            }

            // Make exe a required parameter
            if (string.IsNullOrWhiteSpace(exe))
            {
                Console.WriteLine("Error: /EXEFilename is a required parameter.");
                return;
            }

            // Launch services such as services.msc or gpedit.msc from mmc.exe
            if (exe.Split('.').Last() == "msc")
            {
                // mmc.exe /s services.msc
                string serviceName = exe;
                arguments = $"/s {serviceName}";
                exe = "mmc.exe";
            }

            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath.Replace("\"", "")))
            {
                try
                {
                    dirPath = Environment.CurrentDirectory;
                }
                catch (Exception)
                {
                    dirPath = "";
                }
            }

            string isWindowString = isWindow ? "" : " /NoWindow";

            if (StartTiService())
            {
                string command = $" /SwitchTI{isWindowString} /Dir:\"{dirPath.Replace(@"\", @"\\")}\\\" /Run:\"{exe}\" {arguments}";
                LegendaryTrustedInstaller.RunWithTokenOf("winlogon.exe", true, Application.ExecutablePath, command, isWindow);
            }
        }

        public static void DestroyFileOrFolder(string path)
        {
            string exe = "powershell.exe";
            string arguments = "-NoProfile -NoLogo -ExecutionPolicy Unrestricted";
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

            string deleteCommand = $"-Command \"{elevatePrivilagesCommand}; Remove-Item -Path " + formattedCommandPath;

            deleteCommand += " -Recurse -Force -ErrorAction SilentlyContinue\"";

            arguments += " " + deleteCommand;

            if (StartTiService())
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

        static void ParseCmdLine(string[] args)
        {
            string ExeToRun = "", Arguments = "", WorkingDir = "", toRun = "";

            // args[] can't process DirPath and ExeToRun containing '\'
            // and that will influence the other argument too :(
            // so I need to do it myself :/
            string CmdLine = Environment.CommandLine;
            
            bool isWindow = CmdLine.ToLower().IndexOf("/nowindow") == -1;
            int iToRun = CmdLine.ToLower().IndexOf("/run:");
            if (iToRun != -1)
            {
                toRun = CmdLine.Substring(iToRun + 5).Trim();
                // Process toRun
                int iDQuote1, iDQuote2;
                iDQuote1 = toRun.IndexOf("\"");
                // If a pair of double quote is exist
                if (iDQuote1 != -1)
                {
                    toRun = toRun.Substring(iDQuote1 + 1);
                    iDQuote2 = toRun.IndexOf("\"");
                    if (iDQuote2 != -1)
                    {
                        // before 2nd double quote is ExeToRun, after is Arguments
                        ExeToRun = toRun.Substring(0, iDQuote2);
                        Arguments = toRun.Substring(iDQuote2 + 1);
                    }
                }
                else
                {
                    // before 1st Space is ExeToRun, after is Arguments
                    int firstSpace = toRun.IndexOf(" ");
                    if (firstSpace == -1) { ExeToRun = toRun; }
                    else
                    {
                        ExeToRun = toRun.Substring(0, firstSpace);
                        Arguments = toRun.Substring(firstSpace + 1);
                    }
                }
            }

            // Process all optional arguments before toRun, '/' as separator
            if (iToRun != -1)
                CmdLine = CmdLine.Substring(0, iToRun) + "/";

            string cmdline = CmdLine.ToLower();

            string tmp;

            int iDir, iNextSlash;

            iDir = cmdline.IndexOf("/dir:");
            if (iDir != -1)
            {
                tmp = CmdLine.Substring(iDir + 5);
                iNextSlash = tmp.IndexOf("/");
                if (iNextSlash != -1)
                {
                    tmp = tmp.Substring(0, iNextSlash);
                    WorkingDir = tmp.Replace("\"", "").Trim();
                }
            }

            LegendaryTrustedInstaller.ForceTokenUseActiveSessionID = true;
            LegendaryTrustedInstaller.RunWithTokenOf("TrustedInstaller.exe", false, ExeToRun, Arguments, isWindow, WorkingDir);
        }

        public static bool StartTiService()
        {
            try
            {
                NativeMethods.TryStartService("TrustedInstaller");
                return true;
            }
            catch (Exception)
            {
                //hmm....
                return false;
            }
        }
    }
}
