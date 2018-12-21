using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ShowWithCastRepository : IShowWithCastRepository
    {
        private readonly ScraperContext _context;

        public ShowWithCastRepository(ScraperContext context)
        {
            _context = context;
        }

        public async Task<IList<Show>> GetPageAsync(int page, int size, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            
            return await _context.Shows
                .OrderBy(e => e.Id)
                .Skip(page * size)
                .Take(size)
                .Include(e => e.ShowCasts)
                .ThenInclude(e => e.Person)
                .ToListAsync(ct);
        }

        public async Task<Show> GetAsync(long id, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            
            return await _context.Shows
                .Include(s => s.ShowCasts)
                .ThenInclude(sc => sc.Person)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }
    }
}