using System.Threading.Tasks;
using GarageGroup.Infra;

namespace GarageGroup.Platform.Ping;

public static class Program
{
    public static Task Main(string[] args)
        =>
        Application.UsePingHandler().RunConsoleAsync("In", args);
}