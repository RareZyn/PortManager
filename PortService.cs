using System.Diagnostics;
using System.Text.RegularExpressions;
using PortManager.Models;

namespace PortManager;

public static class PortService
{
    private static readonly Dictionary<int, string> CommonPorts = new()
    {
        [3000] = "React / Next.js Dev Server",
        [3001] = "React Alt / Proxy",
        [4200] = "Angular Dev Server",
        [5000] = "ASP.NET / Flask",
        [5173] = "Vite Dev Server",
        [5174] = "Vite Alt",
        [5500] = "Live Server (VS Code)",
        [8000] = "Django / PHP",
        [8080] = "HTTP Proxy / Tomcat",
        [8081] = "HTTP Alt",
        [8443] = "HTTPS Alt",
        [8888] = "Jupyter Notebook",
        [9000] = "PHP-FPM / SonarQube",
        [27017] = "MongoDB",
        [3306] = "MySQL",
        [5432] = "PostgreSQL",
        [6379] = "Redis",
    };

    public static Dictionary<int, string> GetCommonPorts() => new(CommonPorts);

    public static async Task<Dictionary<int, (int pid, string processName)>> ScanPortsAsync()
    {
        var result = new Dictionary<int, (int pid, string processName)>();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null) return result;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex(@"\s+(TCP|UDP)\s+[\d\.]+:(\d+)\s+[\d\.:\*]+\s+LISTENING\s+(\d+)", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success) continue;

                var port = int.Parse(match.Groups[2].Value);
                var pid = int.Parse(match.Groups[3].Value);

                if (result.ContainsKey(port)) continue;

                var processName = "";
                try
                {
                    using var proc = Process.GetProcessById(pid);
                    processName = proc.ProcessName;
                }
                catch
                {
                    processName = "Unknown";
                }

                result[port] = (pid, processName);
            }
        }
        catch
        {
            // Silently handle errors - will show ports as not in use
        }

        return result;
    }

    public static async Task<List<PortInfo>> GetPortInfosAsync(List<(int port, string name, bool isCustom)> ports)
    {
        var activeConnections = await ScanPortsAsync();
        var infos = new List<PortInfo>();

        foreach (var (port, name, isCustom) in ports)
        {
            var info = new PortInfo
            {
                Port = port,
                Name = name,
                IsCustom = isCustom,
            };

            if (activeConnections.TryGetValue(port, out var conn))
            {
                info.InUse = true;
                info.Pid = conn.pid;
                info.ProcessName = conn.processName;
            }

            infos.Add(info);
        }

        return infos;
    }

    public static bool KillProcess(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            process.Kill(entireProcessTree: true);
            return true;
        }
        catch
        {
            try
            {
                using var process = Process.GetProcessById(pid);
                process.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
