using ConsoleAppNet5.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace ConsoleAppNet5.Services
{
    public interface IDemoService
    {
        Task<bool> DoWork();
    }

    public class DemoService : IDemoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DemoService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly DemoOptions _options;
        private readonly ExtraOptions _extraOptions;

        public DemoService(
             IHttpClientFactory httpClientFactory,
             ILogger<DemoService> logger,
             TelemetryClient telemetryClient,
            IOptions<DemoOptions> demoOptions,
            IOptions<ExtraOptions> extraOptions
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _options = demoOptions.Value;
            _extraOptions = extraOptions.Value;
        }

        public async Task<bool> DoWork()
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Service is not enabled in config options. Aborted.");
                return false;
            }

            // we can inject and use as many option objects we want
            _logger.LogInformation($"Current consoleId: {_extraOptions.ConsoleId}");

            // this loop is just needed to have enough data to activate Azure live metrics
            for (var i = 0; i <= 10; i++)
            {
                try
                {
                    using (_telemetryClient.StartOperation<RequestTelemetry>("operation"))
                    {
                        _logger.LogWarning("A sample warning message. By default, logs with severity Warning or higher is captured by Application Insights");

                        // IHttpClientFactory is the suggested way to create an HttpClient
                        HttpClient httpClient = _httpClientFactory.CreateClient();
                        httpClient.BaseAddress = _options.BaseUrl;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "a bearer token");

                        // as example, we can open a stream to a file or - like this case - a in-memory content
                        using (MemoryStream csvStream = new MemoryStream(Encoding.UTF8.GetBytes(_options.ContentMessage)))
                        {
                            HttpResponseMessage response = await httpClient.PostAsync("/api/endpoint-to-call", new StreamContent(csvStream));
                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogError($"Method was not successful: [{(int)response.StatusCode}] {response.ReasonPhrase}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(DoWork)} failed");
                }

                Thread.Sleep(1 * 1000);
            }

            _logger.LogInformation($"{nameof(DoWork)} completed.");

            _telemetryClient.Flush();

            // As noted from: https://github.com/microsoft/ApplicationInsights-dotnet/issues/407
            // the Flush() is not blocking and doesn't guarantee that the logs are sent.
            // Adding a Sleep as suggested from the docs, waiting for a proper fix in the AI package.
            // NB. There is not an ufficial sleep-time suggested, so tweak this value as you need.
            Console.WriteLine("Waiting to flush logs...");
            Thread.Sleep(10 * 1000);

            return true;
        }
    }
}
