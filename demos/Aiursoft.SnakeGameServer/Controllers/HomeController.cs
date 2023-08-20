using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.WebExtends;
using Microsoft.AspNetCore.Mvc;
using Action = Aiursoft.SnakeGame.Models.Action;

namespace Aiursoft.SnakeGameServer.Controllers
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
            return Ok();
        }

        [Route("repo.ares")]
        public Task<IActionResult> ReturnRepoDemo(string start)
        {
            var repo = _repositoryContainer.GetLogItemRepository();
            return HttpContext.RepositoryAsync(repo, start);
        }
    }

    public class RepositoryContainer
    {
        private readonly object _obj = new object();
        private Repository<Action> _logItemRepository;

        public Repository<Action> GetLogItemRepository()
        {
            lock (_obj)
            {
                if (_logItemRepository == null)
                {
                    _logItemRepository = new Repository<Action>();
                }
            }
            return _logItemRepository;
        }
    }
}
