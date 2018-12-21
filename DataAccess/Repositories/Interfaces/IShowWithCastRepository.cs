using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface IShowWithCastRepository
    {
        Task<IList<Show>> GetPageAsync(int page, int size, CancellationToken ct);

        Task<Show> GetAsync(long id, CancellationToken ct);
    }
}