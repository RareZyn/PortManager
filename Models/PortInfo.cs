namespace PortManager.Models;

public class PortInfo
{
    public int Port { get; set; }
    public string Name { get; set; } = "";
    public string Protocol { get; set; } = "TCP";
    public bool InUse { get; set; }
    public int Pid { get; set; }
    public string ProcessName { get; set; } = "";
    public bool IsCustom { get; set; }
}
