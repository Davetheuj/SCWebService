using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SCWebService.MongoDBSettings;
using SCWebService.Models.UserService;

namespace SCWebService.Services;
public class MongoDBUserService
{
    private readonly IMongoCollection<User> _usersCollection;
    public MongoDBUserService(IOptions<MongoDBUserSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _usersCollection = database.GetCollection<User>(mongoDBSettings.Value.CollectionName);
    }
    public async Task<User?> GetAsyncUnsecured(string userName)
    {
        User user = await _usersCollection.Find(x => x.userName == userName).FirstOrDefaultAsync();
        if(user == null)
        {
            return null;
        }
        else
        {
            user.userPassword = "";
        }
        return user;
    }

    public async Task<User?> GetAsyncSecure(User user)
    {
      return await _usersCollection.Find(x => 
        x.userName == user.userName && 
        x.userPassword == user.userPassword)
        .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User newUser)
    {
        await _usersCollection.InsertOneAsync(newUser);
    }

    public async Task UpdateAsyncUnsecure(User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.userName == updatedUser.userName, updatedUser);

    public async Task UpdateAsyncSecure(User updatedUser) =>
       await _usersCollection.ReplaceOneAsync(x =>
       x.userName == updatedUser.userName &&
       x.userPassword == updatedUser.userPassword, 
       updatedUser);
}