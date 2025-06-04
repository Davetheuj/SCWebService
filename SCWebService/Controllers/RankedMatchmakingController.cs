using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SCWebService.Models.MatchmakingService;
using SCWebService.Services.Matchmaking;
using SCWebService.Services.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SCWebService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RankedMatchmakingController : ControllerBase
    {

        private readonly IRankedMatchmakingService _matchmakingService;
        private readonly IUserService _userService;

        public RankedMatchmakingController(IRankedMatchmakingService matchmakingService, IUserService userService)
            {
                _matchmakingService = matchmakingService;
                _userService = userService;
            }

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

        [HttpPost("/ranked_mm/get_match_token/{userId}")]
        public IActionResult StartMatch(string userId)
        {
            var claims = new[]
            {
                new Claim("userID", userId),
                new Claim("start", DateTime.UtcNow.ToString("O"))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "SCWebService",
                audience: "SCClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: creds
            );

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        [HttpPost("/ranked_mm/submit_match_result")]
        public async Task<IActionResult> PostMatchUpdate([FromBody] MatchSubmission submission)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!));

            try
            {
                var principal = handler.ValidateToken(submission.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "SCWebService",
                    ValidateAudience = true,
                    ValidAudience = "SCClient",
                    ValidateLifetime = true,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromSeconds(10)
                }, out var validatedToken);

                var userId = principal.FindFirst("userID")?.Value;
                var startTime = DateTime.Parse(principal.FindFirst("start")?.Value ?? "").ToUniversalTime();

                Console.WriteLine("Start Time: " + startTime);
                Console.WriteLine($"Total Seconds: {(DateTime.UtcNow - startTime.ToUniversalTime()).TotalSeconds}");
                // Validate result
                if ((DateTime.UtcNow - startTime).TotalSeconds < 1)
                {
                    return BadRequest("Invalid match data");
                }

                Console.WriteLine("User ID: " + userId);
                var user = await _userService.GetAsyncSecure(userId!);
                if (user == null) return NotFound();

                //Update the user here
                int gems = MatchSubmission.CalculateRewards(submission.Victory);
                user.gems += gems;
                if (submission.Ranked)
                {
                    int mmrChange = MatchSubmission.CalculateMMRChange(submission.LocalMMR, submission.OppositionMMR, submission.Victory);
                    user.userMMR += mmrChange;
                }
                if (submission.Victory)
                {
                    user.wins += 1;
                }
                else
                {
                    user.losses += 1;
                }
                await _userService.UpdateAsyncSecure(user);

                return Accepted(gems);
            }
            catch (Exception)
            {
                return Unauthorized("Invalid or expired token.");
            }
        }
    }

}
