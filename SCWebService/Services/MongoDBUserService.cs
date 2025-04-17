using SCWebService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
    public async Task<User?> GetAsyncUnsecured(string userName) =>
        await _usersCollection.Find(x => x.userName == userName).FirstOrDefaultAsync();

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

    public async Task UpdateAsync(User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.userName == updatedUser.userName, updatedUser);
}