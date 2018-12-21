using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Configuration;
using DataAccess.Entities;
using DataAccess.HttpClients.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataAccess.HttpClients
{
    public class ScraperHttpClient : IScraperHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ScraperHttpClient> _logger;
        private readonly ScraperOptions _options;

        public ScraperHttpClient(ILogger<ScraperHttpClient> logger,
            IOptions<ScraperOptions> options,
            HttpClient httpClient)
        {
            _options = options.Value;
            _logger = logger;
            _client = httpClient;
        }

        public async Task<IEnumerable<Show>> ScrapeShowsAsync(int page, CancellationToken cancellationToken)
        {
            var requestUri = string.Format(_options.ShowsUri, page);
            var response = await _client.GetAsync(requestUri, cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var dynamicResult = JsonConvert.DeserializeObject<List<dynamic>>(json);
                var shows = dynamicResult.Select(s => new Show
                {
                    Id = (int) s.id,
                    Name = (string) s.name
                });

                return shows;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Response status code does not indicate success: {response.StatusCode} " +
                                    $"on request: {_client.BaseAddress}{requestUri}");
                throw;
            }
        }

        public async Task<IEnumerable<Person>> ScrapeShowCastAsync(long showId, CancellationToken cancellationToken)
        {
            var requestUri = string.Format(_options.CastUri, showId);
            var response = await _client.GetAsync(requestUri, cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var jObjectResult = JObject.Parse(json);
                var creditedPerson =
                    from p in jObjectResult["_embedded"]["cast"]
                    select new Person
                    {
                        Id = (long) p["person"]["id"],
                        Name = (string) p["person"]["name"],
                        Birthday = ToDateTime((string) p["person"]["birthday"])
                    };

                return creditedPerson;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Response status code does not indicate success: {response.StatusCode} " +
                                    $"on request: {_client.BaseAddress}{requestUri}");
                throw;
            }
        }

        private DateTime? ToDateTime(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            if (DateTime.TryParse(input, out var result)) return result;

            return null;
        }
    }
}