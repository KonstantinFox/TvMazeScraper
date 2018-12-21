using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface IScrapeRepository
    {
        Task<long> GetMaxShowIndexAsync(CancellationToken cancellationToken);

        Task BulkInsertShowsAsync(IEnumerable<Show> shows, CancellationToken ct);

        Task BulkInsertPersonAsync(IEnumerable<Person> persons, CancellationToken ct);

        Task BulkInsertShowPersonRelationsAsync(IEnumerable<ShowPerson> showPersons, CancellationToken ct);
    }
}