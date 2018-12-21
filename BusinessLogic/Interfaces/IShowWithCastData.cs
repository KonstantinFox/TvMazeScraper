using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Model;

namespace BusinessLogic.Interfaces
{
    public interface IShowWithCastData
    {
        Task<IList<Show>> GetPageAsync(int page, int size, CancellationToken ct);

        Task<Show> GetAsync(long id, CancellationToken ct);
    }
}