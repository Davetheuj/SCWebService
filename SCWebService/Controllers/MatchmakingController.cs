using Microsoft.AspNetCore.Mvc;
using SCWebService.Models.MatchmakingService;
using SCWebService.Services;
using System.Threading.Tasks;

namespace SCWebService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MatchmakingController : ControllerBase
    {

        private readonly MatchmakingService _matchmakingService;

        public MatchmakingController(MatchmakingService matchmakingService) =>
            _matchmakingService = matchmakingService;

        [HttpPost("/find_match")]
        public async Task<IActionResult> Get([FromBody] string userName, [FromBody] int userMMR)
        {
            var mmUser = await _matchmakingService.FindValidHostAsync(userName, userMMR);
            
            JsonResult result = new JsonResult("");

            if (mmUser != null)
            {
                result.StatusCode = 204; //204 is no content but successful request
            }
            else
            {
                result.Value = mmUser!.JoinCode;
                result.StatusCode = 200;
            }

            return result;
        }

        [HttpPost("/add_host")]
        public async Task<IActionResult> Post(MatchmakingUser mmUser)
        {
            await _matchmakingService.CreateAsync(mmUser);
            return Accepted();
        }
     

        [HttpPost("/remove_from_queue")]
        public async Task<IActionResult> RemoveFromQueue(MatchmakingUser mmUser)
        {
            bool success = await _matchmakingService.TryRemoveFromQueue(mmUser);
            if (success)
            {
                return Accepted();
            }
            else
            {
                return StatusCode(503);
            }
        }
    }
}
