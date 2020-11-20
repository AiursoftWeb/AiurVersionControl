using AiurEventSyncer.Models;
using AiurEventSyncer.WebExtends;
using Microsoft.AspNetCore.Mvc;
using SampleWebApp.Data;
using SampleWebApp.Services;
using System.Threading.Tasks;

namespace SampleWebApp.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly RepoFactory _repoFactory;
        private static Repository<LogItem> repository;
        private static object _obj = new object();

        public HomeController(
            RepoFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        public IActionResult Index()
        {
            return Ok(new { Message = "Welcome!" });
        }

        [Route("repo.are")]
        public Task<IActionResult> ReturnRepoDemo()
        {
            lock (_obj)
            {
                if (repository == null)
                {
#warning Use a singleton pool.
                    repository = _repoFactory.BuildRepo<LogItem>();
                }
            }
            return this.BuildWebActionResultAsync(repository);
        }
    }
}
