using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.Model;
using DataAccess.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ShowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly IShowWithCastData _data;
        private readonly ShowApiOptions _options;

        public ShowsController(IShowWithCastData data, IOptions<ShowApiOptions> options)
        {
            _data = data;
            _options = options.Value;
        }

        /// <summary>
        ///     Returns list of Shows ordered by Id with Cast info ordered by descending Birthday
        /// </summary>
        /// <param name="page">Specifies page to show, page &gt;= 0</param>
        /// <param name="size">Specifies size of page to show, 0 &lt;= size &lt;= 250</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Show>>> Get(int page, int size, CancellationToken ct)
        {
            size = EnsureSizeValid(size);
            if (size == 0) return Ok(Enumerable.Empty<Show>());
            if (page < 0) return NotFound();
            var result = await _data.GetPageAsync(page, size, ct);
            if (result == null || !result.Any()) return NotFound();
            return Ok(result);
        }

        /// <summary>
        ///     Returns specified Show with Cast information ordered by descending Birthday
        /// </summary>
        /// <param name="id">Show id, id &gt; 0</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Show>> Get(long id, CancellationToken ct)
        {
            var result = await _data.GetAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        private int EnsureSizeValid(int size)
        {
            if (size > _options.MaxPageSize) size = _options.MaxPageSize;
            return size < 0 ? 0 : size;
        }
    }
}