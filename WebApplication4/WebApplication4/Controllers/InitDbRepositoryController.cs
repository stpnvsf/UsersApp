using Application;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers
{
    [Route("/init")]
    [ApiController]
    public sealed class InitDbRepositoryController : ControllerBase
    {

        private readonly IInitDb _initDb;

        public InitDbRepositoryController(IInitDb initDb)
        {
            _initDb = initDb;
        }

        [HttpPost]
        public async Task<ActionResult> Init()
        {
            await _initDb.Init();

            return Ok();
        }
    }
}
