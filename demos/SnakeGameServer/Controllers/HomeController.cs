using AiurEventSyncer.Models;
using AiurEventSyncer.WebExtends;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SnakeGameServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly RepositoryContainer _repositoryContainer;

        public HomeController(RepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("repo.ares")]
        public Task<IActionResult> ReturnRepoDemo(string start)
        {
            var repo = _repositoryContainer.GetLogItemRepository();
            return new ActionBuilder().BuildWebActionResultAsync(HttpContext.WebSockets, repo, start);
        }
    }

    public class RepositoryContainer
    {
        private readonly object _obj = new object();
        private Repository<string> _logItemRepository;

        public Repository<string> GetLogItemRepository()
        {
            lock (_obj)
            {
                if (_logItemRepository == null)
                {
                    _logItemRepository = new Repository<string>();
                }
            }
            return _logItemRepository;
        }
    }
}
