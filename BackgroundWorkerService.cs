using Microsoft.Extensions.Logging;
namespace dotnet_project.services
{ 
public class BackgroundWorkerService : BackgroundService 
{
    private readonly ILogger<BackgroundService> _logger;

    public BackgroundWorkerService(ILogger<BackgroundService> logger)
    {
        _logger = logger;
    }


    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(cancellationToken);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {

            await Task.Delay(2000, stoppingToken);
           _logger.LogInformation("Worker running at: {0}", DateTime.Now.ToString());

            //await ProcessDataAsync();
            //await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Repeat every 24 hours.
        }
    }


    private Task ProcessDataAsync()
    {
        // Imagine complex data processing here.
        Console.WriteLine("Processing data...");
        return Task.CompletedTask;
    }
  }

}