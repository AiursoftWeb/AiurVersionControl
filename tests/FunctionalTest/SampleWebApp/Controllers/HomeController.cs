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

        [Route("repo.are")]
        public Task<IActionResult> ReturnRepoDemo()
        {
            var repository = _repoFactory.BuildRepo();
            return this.BuildWebActionResultAsync(repository);
        }
    }
}
