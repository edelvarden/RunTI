using System.Diagnostics;

namespace RunTI
{
    internal class NetworkManager
    {
        public static void ToggleAdapters(bool isEnabled = true)
        {
            ToggleAdapter("Ethernet", isEnabled);
            ToggleAdapter("Ethernet0", isEnabled);
        }

        private static void ToggleAdapter(string interfaceName, bool isEnabled)
        {
            string status = isEnabled ? "enable" : "disable";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface set interface \"{interfaceName}\" {status}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    p.Start();
                }
            }
            catch{}
        }
    }
}
