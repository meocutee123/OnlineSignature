using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineSignature.Services;
using OnlineSignature.Ultilities;
using System.Threading.Tasks;

namespace OnlineSignature.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        private readonly IExampleService _exampleService;
        public ExampleController(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }
        [HttpGet]
        [Route("~/ds/callback")]
        public IActionResult Callback(string code)
        {
            var (oAuthToken, defaultAccount) = _exampleService.RequestAccessToken(code);

            HttpContext.Session.SetObject("oAuthToken", oAuthToken);

            HttpContext.Session.SetObject("account", defaultAccount);

            return Redirect("https://localhost:44333/");
        }


        [HttpGet]
        [Route("~/ds/refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {

            var (oAuthToken, defaultAccount) = await _exampleService.RefreshToken(refreshToken);

            HttpContext.Session.SetObject("oAuthToken", oAuthToken);

            HttpContext.Session.SetObject("account", defaultAccount);

            return Redirect("https://localhost:44333/");
        }
    }
}
