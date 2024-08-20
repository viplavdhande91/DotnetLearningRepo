using Microsoft.AspNetCore.Mvc;

namespace dotnet_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        //public async Task<string> GetDataWithTimeoutAsync(TimeSpan timeout)
        //{
        //    var tcs = new TaskCompletionSource<string>();

        //    // Simulate async work
        //    var workTask = Task.Run(async () =>
        //    {
        //        await Task.Delay(5000); // Simulating long-running task
        //        tcs.TrySetResult("Data retrieved");
        //    });

        //    if (await Task.WhenAny(workTask, Task.Delay(timeout)) == workTask)
        //    {
        //        return await tcs.Task; // The workTask completed within timeout
        //    }
        //    else
        //    {
        //        tcs.TrySetCanceled(); // Timeout occurred
        //        throw new TimeoutException("The operation timed out.");
        //    }
        //}


    }
}
