namespace dotnet_install
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using VQCore.Infrastructure.ProcessHost;

    class Program
    {
        private static readonly string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "install");

        private const string filePath = "dotnet-install.sh";

        private static readonly string installPath = Path.Combine(outputDirectory, filePath);

        static async Task Main(string[] args)
        {
            //Download each time in case updates have been added to it
            await DownloadInstallScript();

            //if dotnet already installed somewhere else then put it there
            args = args.Concat(new[] { "--install-dir", Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName }).ToArray();

            var procHandler = new ChildProcessHandler("/bin/bash", $"{installPath} {string.Join(" ", args)}");

            procHandler.OnErrorDataReceived = (_, data) =>
                Console.WriteLine(data.Data);

            procHandler.OnExited = (_, data) =>
                Console.WriteLine($"Finished with exit code: {procHandler.ExitCode}");

            procHandler.OnOutputDataReceived = (o, eventArgs) => { Console.WriteLine(eventArgs.Data); };

            procHandler.Run();
        }

        static async Task DownloadInstallScript()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://dot.net/v1/");
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var requestUrl = "dotnet-install.sh";

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    var sendTask = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    var response = sendTask.EnsureSuccessStatusCode();
                    var httpStream = await response.Content.ReadAsStreamAsync();

                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    using (var fileStream = File.Create(installPath))
                    {
                        httpStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
