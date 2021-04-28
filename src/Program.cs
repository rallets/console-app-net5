using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ConsoleAppNet5
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                    .AddLogging()
                    .AddConfiguration()
                    .AddKeyVault()
                    .DumpConfiguration()
                    .AddServices()
                    .RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
            }
        }

    }
}
