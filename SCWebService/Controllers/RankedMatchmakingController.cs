using Microsoft.AspNetCore.Mvc;
using SCWebService.Models.MatchmakingService;
using SCWebService.Services;
using System.Threading.Tasks;

namespace SCWebService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RankedMatchmakingController : ControllerBase
    {

        private readonly RankedMatchmakingService _matchmakingService;

        public RankedMatchmakingController(RankedMatchmakingService matchmakingService) =>
            _matchmakingService = matchmakingService;

        [HttpPost("/ranked_mm/find_match")]
        public async Task<IActionResult> Get(RankedMatchmakingUser user)
        {
            var mmUser = await _matchmakingService.FindValidHostAsync(user.UserName, user.UserMMR);
            
            JsonResult result = new JsonResult("");

            if (mmUser == null)
            {
                result.StatusCode = 204; //204 is no content but successful request
            }
            else
            {
                result.Value = mmUser;
                result.StatusCode = 200;
            }

            return result;
        }

        [HttpPost("/ranked_mm/add_host")]
        public async Task<IActionResult> Post(RankedMatchmakingUser mmUser)
        {
            await _matchmakingService.CreateAsync(mmUser);
            return Accepted();
        }
     

        [HttpPost("/ranked_mm/remove_from_queue/{username}")]
        public async Task<IActionResult> RemoveFromQueue(string username)
        {
            bool success = await _matchmakingService.TryRemoveFromQueue(username);
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
