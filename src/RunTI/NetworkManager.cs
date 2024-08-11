using System;
using System.Diagnostics;
using System.Media;
using System.Threading.Tasks;

namespace RunTI
{
    internal class NetworkManager
    {
        private static readonly string[] Adapters = { "Ethernet", "Ethernet0" };

        public static async Task ToggleAdaptersAsync(bool isEnabled = true)
        {
            var tasks = new Task[Adapters.Length];
            for (int i = 0; i < Adapters.Length; i++)
            {
                tasks[i] = ToggleAdapterAsync(Adapters[i], isEnabled);
            }
            await Task.WhenAll(tasks);
        }

        private static async Task ToggleAdapterAsync(string interfaceName, bool isEnabled)
        {
            string status = isEnabled ? "enable" : "disable";
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface set interface \"{interfaceName}\" {status}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var p = new Process { StartInfo = psi })
                {
                    p.Start();

                    string output = await p.StandardOutput.ReadToEndAsync();
                    string error = await p.StandardError.ReadToEndAsync();

                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        Console.WriteLine($"Failed to {status} adapter {interfaceName}: {error}");
                    }
                    else
                    {
                        //if (isEnabled)
                        //{
                        //    SystemSounds.Asterisk.Play();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling adapter {interfaceName}: {ex.Message}");
            }
        }

        public static async Task ReloadAdaptersAsync()
        {
            await ToggleAdaptersAsync(false);
            await Task.Delay(1000);
            await ToggleAdaptersAsync(true);
        }
    }
}
