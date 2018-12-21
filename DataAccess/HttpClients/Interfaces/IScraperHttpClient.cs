using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.HttpClients.Interfaces
{
    public interface IScraperHttpClient
    {
        Task<IEnumerable<Show>> ScrapeShowsAsync(int page, CancellationToken cancellationToken);

        Task<IEnumerable<Person>> ScrapeShowCastAsync(long showId, CancellationToken cancellationToken);
    }
}