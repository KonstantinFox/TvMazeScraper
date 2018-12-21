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
    public class ScrapeRepository : IScrapeRepository
    {
        private readonly ScraperContext _context;

        public ScrapeRepository(ScraperContext context)
        {
            _context = context;
        }

        public async Task<long> GetMaxShowIndexAsync(CancellationToken cancellationToken)
        {
            if (!await _context.Shows.AnyAsync(cancellationToken)) return 0;
            var result = await _context.Shows.MaxAsync(e => e.Id, cancellationToken);
            return result;
        }

        public async Task BulkInsertShowsAsync(IEnumerable<Show> shows, CancellationToken ct)
        {
            var showsToAdd = await DetermineShowsToAddAsync(shows, ct);
            await _context.Shows.AddRangeAsync(showsToAdd, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task BulkInsertPersonAsync(IEnumerable<Person> persons, CancellationToken ct)
        {
            var castToAdd = await DeterminePersonsToAddAsync(persons, ct);
            await _context.Persons.AddRangeAsync(castToAdd, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task BulkInsertShowPersonRelationsAsync(IEnumerable<ShowPerson> showPersons, CancellationToken ct)
        {
            var toAdd = await DetermineShowPersonRelationsToAddAsync(showPersons, ct);
            await _context.ShowPersons.AddRangeAsync(toAdd, ct);
            await _context.SaveChangesAsync(ct);
        }

        private async Task<IEnumerable<Show>> DetermineShowsToAddAsync(IEnumerable<Show> shows, CancellationToken ct)
        {
            var showsList = shows.ToList();
            var ids = showsList.Select(s => s.Id);
            var existingIds = await _context.Shows.Where(s => ids.Contains(s.Id)).Select(s => s.Id).ToListAsync(ct);
            return showsList.Where(s => !existingIds.Contains(s.Id));
        }

        private async Task<IEnumerable<Person>> DeterminePersonsToAddAsync(IEnumerable<Person> cast,
            CancellationToken ct)
        {
            var castList = ToPersonDistinct(cast).ToList();
            var ids = castList.Select(c => c.Id);
            var existingIds = await _context.Persons.Where(p => ids.Contains(p.Id)).Select(p => p.Id).ToListAsync(ct);
            return castList.Where(p => !existingIds.Contains(p.Id));
        }

        private async Task<IEnumerable<ShowPerson>> DetermineShowPersonRelationsToAddAsync(
            IEnumerable<ShowPerson> showPersonRelations, CancellationToken ct)
        {
            var showPersonDistinct = ToShowPersonDistinct(showPersonRelations).ToList();
            var existing = await _context.ShowPersons
                .Where(sc => showPersonDistinct.Select(scr => scr.PersonId).Contains(sc.PersonId))
                .ToListAsync(ct);
            var toAdd = showPersonDistinct.Where(scr =>
                !existing.Any(ex => ex.PersonId == scr.PersonId && ex.ShowId == scr.ShowId));
            return toAdd;
        }

        private IEnumerable<Person> ToPersonDistinct(IEnumerable<Person> persons)
        {
            return persons.GroupBy(p => p.Id).Select(grp => grp.First());
        }

        private IEnumerable<ShowPerson> ToShowPersonDistinct(IEnumerable<ShowPerson> showPersons)
        {
            return showPersons.GroupBy(p => new {p.PersonId, p.ShowId}).Select(grp => grp.First());
        }
    }
}