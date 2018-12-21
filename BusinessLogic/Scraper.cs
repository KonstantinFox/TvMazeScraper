using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using DataAccess.Configuration;
using DataAccess.Entities;
using DataAccess.HttpClients.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic
{
    public class Scraper : IScopedService
    {
        private readonly ILogger<Scraper> _logger;
        private readonly ScraperOptions _options;
        private readonly IScrapeRepository _scrapeRepository;
        private readonly IScraperHttpClient _scraperHttpClient;

        public Scraper(IScraperHttpClient scraperHttpClient, IScrapeRepository scrapeRepository,
            IOptions<ScraperOptions> options, ILogger<Scraper> logger)
        {
            _scraperHttpClient = scraperHttpClient;
            _options = options.Value;
            _logger = logger;
            _scrapeRepository = scrapeRepository;
        }

        //TODO: Implement cancellation
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var currentIndex = await _scrapeRepository.GetMaxShowIndexAsync(cancellationToken);
            // ReSharper disable once PossibleLossOfFraction
            var page = (int) Math.Floor((double) (currentIndex / _options.MazeApiMaxPageSize));
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!await ScrapePage(page, cancellationToken)) break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception thrown when scraping page : {page}");
                    break;
                }

                page++;
            }
        }

        private async Task<bool> ScrapePage(int page, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            var shows = (await _scraperHttpClient.ScrapeShowsAsync(page, cancellationToken)).ToList();
            if (!shows.Any()) return false;

            var persons = new List<Person>();
            var showPersonsRelations = new List<ShowPerson>();
            foreach (var show in shows)
            {
                var cast = (await _scraperHttpClient.ScrapeShowCastAsync(show.Id, cancellationToken)).ToList();
                var showPersons = cast.Select(c => new ShowPerson {PersonId = c.Id, ShowId = show.Id});
                persons.AddRange(cast);
                showPersonsRelations.AddRange(showPersons);
            }

            await _scrapeRepository.BulkInsertShowsAsync(shows, cancellationToken);
            await _scrapeRepository.BulkInsertPersonAsync(persons, cancellationToken);
            await _scrapeRepository.BulkInsertShowPersonRelationsAsync(showPersonsRelations, cancellationToken);
            return true;
        }
    }
}