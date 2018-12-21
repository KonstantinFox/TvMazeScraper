using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic;
using DataAccess.Configuration;
using DataAccess.Entities;
using DataAccess.HttpClients.Interfaces;
using DataAccess.Repositories.Interfaces;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tests
{
    [TestFixture]
    public class ScraperTests
    {
        private IScraperHttpClient FakeHttpClient => A.Fake<IScraperHttpClient>();

        private IOptions<ScraperOptions> FakeOptions => A.Fake<IOptions<ScraperOptions>>();

        private ILogger<Scraper> FakeLogger => A.Fake<ILogger<Scraper>>();

        private IScrapeRepository FakeRepository => A.Fake<IScrapeRepository>();

        [Test]
        public async Task LooksUpCurrentShowIndex()
        {
            //arrange
            var repo = FakeRepository;
            var sut = BuildSut(scrapeRepository: repo);
            
            //act
            await sut.RunAsync(CancellationToken.None);
            
            //assert
            A.CallTo(() => repo.GetMaxShowIndexAsync(A<CancellationToken>._)).MustHaveHappened();
        }

        [Test]
        public async Task SkipsAlreadyScrapedPages()
        {
            //arrange 
            var repo = FakeRepository;
            var client = FakeHttpClient;
            A.CallTo(() => repo.GetMaxShowIndexAsync(A<CancellationToken>._)).Returns(Task.FromResult((long)751));
            var sut = BuildSut(scrapeRepository: repo, scraperHttpClient: client);
            
            //act
            await sut.RunAsync(CancellationToken.None);

            //assert
            A.CallTo(() => client.ScrapeShowsAsync(3, A<CancellationToken>._)).MustHaveHappened();
        }

        [Test]
        public async Task ScrapesCastListForEachShow()
        {
            //arrange
            var repo = FakeRepository;
            A.CallTo(() => repo.GetMaxShowIndexAsync(A<CancellationToken>._)).Returns(Task.FromResult<long>(0));
            var client = FakeHttpClient;
            var showsFromClient = 10;
            A.CallTo(() => client.ScrapeShowsAsync(0, A<CancellationToken>._)).Returns(Task.FromResult(GetShows(showsFromClient)));
            var sut = BuildSut(scrapeRepository: repo, scraperHttpClient: client);

            //act
            await sut.RunAsync(CancellationToken.None);

            //assert
            A.CallTo(() => client.ScrapeShowCastAsync(A<long>._, A<CancellationToken>._))
                .MustHaveHappened(showsFromClient, Times.Exactly);
        }
        
        private Scraper BuildSut(IScraperHttpClient scraperHttpClient = null,
            IScrapeRepository scrapeRepository = null,
            IOptions<ScraperOptions> options = null,
            ILogger<Scraper> logger = null)
        {
            if (scraperHttpClient == null)
            {
                scraperHttpClient = FakeHttpClient;
            }

            if (scrapeRepository == null)
            {
                scrapeRepository = FakeRepository;
            }

            if (options == null)
            {
                options = FakeOptions;
                var scraperOptions = new ScraperOptions()
                {
                    MazeApiMaxPageSize = 250
                };
                A.CallTo(() => options.Value).Returns(scraperOptions);
            }

            if (logger == null)
            {
                logger = FakeLogger;
            }
            
            return new Scraper(scraperHttpClient, scrapeRepository, options, logger);
        }

        private IEnumerable<Show> GetShows(int amount)
        {
            var result = A.CollectionOfFake<Show>(amount);
            for (var i = 0; i < result.Count; i++)
            {
                result[i].Id = i;
                yield return result[i];
            }
        }
    }
}