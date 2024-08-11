using System;
using System.Linq;
using System.Management;
using System.Media;
using System.Threading.Tasks;

namespace RunTI
{
    internal class NetworkManager
    {
        private static readonly string[] Adapters = { "Ethernet", "Ethernet0" };

        private static void ToggleAdapter(string adapterName, bool isEnabled)
        {
            string action = isEnabled ? "Enable" : "Disable";
            try
            {
                string query = $"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = '{adapterName}'";
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var adapter = (ManagementObject)obj;
                        var result = adapter.InvokeMethod(action, null);
   
                        //if(result != null)
                        //{
                        //    SystemSounds.Asterisk.Play();
                        //}
                    }
                }
            }
            catch{}
        }

        public static async Task ToggleAdaptersAsync(bool isEnabled = true)
        {
            var tasks = Adapters.Select(adapter => Task.Run(() => ToggleAdapter(adapter, isEnabled)));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task ReloadAdaptersAsync()
        {
            await ToggleAdaptersAsync(false);
            await Task.Delay(500);
            await ToggleAdaptersAsync(true);
        }
    }
}
