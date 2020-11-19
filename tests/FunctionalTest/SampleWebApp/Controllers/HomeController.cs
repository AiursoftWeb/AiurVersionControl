using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AiurEventSyncer.WebExtends;
using SampleWebApp.Data;
using SampleWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
