using SCWebService.Models.MatchmakingService;
using SCWebService.Models.UserService;

namespace SCWebService.Services.Matchmaking
{
    public interface IRankedMatchmakingService
    {
        Task<RankedMatchmakingUser?> FindValidHostAsync(string userName, int userMMR);
        Task CreateAsync(RankedMatchmakingUser newUser);
        Task<bool> TryRemoveFromQueue(string username);
    }

}
