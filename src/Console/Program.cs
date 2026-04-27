using System.Threading.Tasks;
using GarageGroup.Infra;

namespace GarageGroup.Platform.Ping;

static class Program
{
    static Task Main(string[] args)
        =>
        Application.UsePingHandler().UseConsoleRunner("In", args).RunAsync();
}