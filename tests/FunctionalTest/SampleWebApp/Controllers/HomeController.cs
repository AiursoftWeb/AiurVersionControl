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
            var repo = _repoFactory.BuildRepo<LogItem>();
            return this.BuildWebActionResultAsync(repo);
        }
    }
}
