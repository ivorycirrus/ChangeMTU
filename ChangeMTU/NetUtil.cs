using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace ChangeMTU
{
    class NetUtil
    {
        /**
         * Retrieve ethernet adapters
         * */
        public static void RetrieveEthernetAdapterList(Action<List<EthernetAdapter>> onListRefreshed)
        {
            List<string> netshResult = new List<string>();

            Process netsh = new Process
            {
                StartInfo = {
                     FileName = @"netsh",
                     Arguments = @"interface ipv4 show interfaces",
                     UseShellExecute = false,
                     RedirectStandardOutput = true,
                     CreateNoWindow = true
                }
            };
            netsh.EnableRaisingEvents = true;
            netsh.OutputDataReceived += (s, eo) => netshResult.Add(eo.Data);
            netsh.Exited += (s, ef) => {
                var ethList = parseAdapterListCmd(netshResult);
                onListRefreshed?.Invoke(ethList);
            };
            netsh.Start();
            netsh.BeginOutputReadLine();
            netsh.WaitForExit();
        }

        /**
         * parse netsh result
         * */
        private static List<EthernetAdapter> parseAdapterListCmd(List<String> rawStr)
        {
            List<EthernetAdapter> ethList = new List<EthernetAdapter>();
            Regex reg = new Regex(@"\s*(\d+)\s*\d+\s*(\d+)\s+(\w+)\s+(.*)");
            foreach (var line in rawStr)
            {
                if (!String.IsNullOrEmpty(line) && reg.IsMatch(line))
                {
                    MatchCollection match = reg.Matches(line);
                    foreach (Match mm in match)
                    {
                        var k = mm.Groups;

                        Int32 index = Convert.ToInt32(mm.Groups[1].ToString());
                        Int64 mtu = Convert.ToInt64(mm.Groups[2].ToString());
                        string status = mm.Groups[3].ToString();
                        string name = mm.Groups[4].ToString();

                        if (!name.Contains("Loopback"))
                        {
                            ethList.Add(new EthernetAdapter(index, mtu, status, name));
                        }
                    }
                }
            }
            return ethList;
        }

        /**
         * Change MTU
         * */
        public static void ChangeMTU(Int32 ethIndex, Int64 MTU, Action onMtuChanged)
        {
            List<string> netshResult = new List<string>();

            Process netsh = new Process
            {
                StartInfo = {
                     FileName = @"netsh",
                     Arguments = String.Format(@"interface ipv4 set subinterface {0} mtu={1} store=persistent", ethIndex, MTU),
                     UseShellExecute = false,
                     CreateNoWindow = true
                }
            };
            netsh.EnableRaisingEvents = true;
            netsh.Exited += (s, ef) => {
                onMtuChanged?.Invoke();
            };          
            netsh.Start();
            netsh.WaitForExit();
        }
    }
}
