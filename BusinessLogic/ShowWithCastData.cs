using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.Model;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic
{
    public class ShowWithCastData : IShowWithCastData
    {
        private readonly IShowWithCastRepository _repository;

        public ShowWithCastData(IShowWithCastRepository repository)
        {
            _repository = repository;
        }

        public async Task<IList<Show>> GetPageAsync(int page, int size, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            var entities = await _repository.GetPageAsync(page, size, ct);
            var result = entities?.Select(ToShowWithCast).ToList();
            return result;
        }

        public async Task<Show> GetAsync(long id, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            
            var entity = await _repository.GetAsync(id, ct);
            return entity == null ? null : ToShowWithCast(entity);
        }

        private Show ToShowWithCast(DataAccess.Entities.Show e)
        {
            return new Show
            {
                Id = e.Id,
                Name = e.Name,
                Cast = e.ShowCasts?
                    .Select(sc => new CastMember
                    {
                        Id = sc.Person.Id,
                        Name = sc.Person.Name,
                        Birthday = sc.Person.Birthday
                    })
                    .OrderByDescending(p => p.Birthday)
                    .ToList()
            };
        }
    }
}