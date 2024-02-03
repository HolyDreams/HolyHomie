using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyHomie.Services
{
    public class RunLavalink : IHostedService
    {
        private Process lavaProcess;
        private readonly ILogger<RunLavalink> _logger;
        public RunLavalink(ILogger<RunLavalink> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запускаю Lavalink...");

            try
            {
                var processList = Process.GetProcessesByName("java");
                if (processList.Length != 0)
                {
                    foreach (var proc in processList)
                    {
                        proc.Kill();
                    }
                }

                string lavalinkFile = Path.Combine(AppContext.BaseDirectory, "Lavalink", "Lavalink.jar");
                if (!File.Exists(lavalinkFile))
                {
                    _logger.LogError("Нет файла ../Lavalink/Lavalink.jar");
                    return Task.CompletedTask;
                }

                var process = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar \"{Path.Combine(AppContext.BaseDirectory, "Lavalink")}/Lavalink.jar\"",
                    WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "Lavalink"),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                lavaProcess = Process.Start(process);
                _logger.LogInformation("Lavalink запущен!");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (lavaProcess != null)
            {
                lavaProcess.Kill();
            }

            return Task.CompletedTask;
        }
    }
}
