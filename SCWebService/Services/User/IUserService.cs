using SCWebService.Models.UserService;

namespace SCWebService.Services.UserService
{
    public interface IUserService
    {
        Task<User?> GetAsyncUnsecured(string userName);
        Task<User?> GetAsyncSecure(User user);
        Task<User?> GetAsyncSecure(string id);
        Task CreateAsync(User newUser);
        Task UpdateAsyncUnsecure(User updatedUser);
        Task UpdateAsyncSecure(User updatedUser);
    }
}
