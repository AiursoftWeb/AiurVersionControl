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
        private readonly RepoFactory<LogItem> _repoFactory;
        public static Repository<LogItem> _repo;
        private static object _obj = new object();

        public HomeController(
            RepoFactory<LogItem> repoFactory)
        {
            _repoFactory = repoFactory;
        }

        public IActionResult Index()
        {
            return Ok(new { Message = "Welcome!" });
        }

        [Route("repo.ares")]
        public Task<IActionResult> ReturnRepoDemo()
        {
            lock (_obj)
            {
                if (_repo == null)
                {
                    _repo = _repoFactory.BuildRepo();
                }
            }
            return this.BuildWebActionResultAsync(_repo);
        }
    }
}
