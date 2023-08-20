using Aiursoft.AiurEventSyncer.WebExtends;
using Microsoft.AspNetCore.Mvc;
using SampleWebApp.Services;
using System.Diagnostics.CodeAnalysis;

namespace SampleWebApp.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly RepositoryContainer _repositoryContainer;

        public HomeController(RepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        [ExcludeFromCodeCoverage]
        public IActionResult Index()
        {
            return Ok(new { Message = "Welcome!" });
        }

        [Route("repo.ares")]
        public Task<IActionResult> ReturnRepoDemo(string start)
        {
            var repo = _repositoryContainer.GetLogItemRepository();
            return HttpContext.RepositoryAsync(repo, start);
        }
    }
}
