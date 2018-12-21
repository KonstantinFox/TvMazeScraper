using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IScopedService
    {
        Task RunAsync(CancellationToken ct);
    }
}