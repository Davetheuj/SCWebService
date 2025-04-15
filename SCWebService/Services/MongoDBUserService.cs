using SCWebService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

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
    public async Task<List<User>> GetAsync() =>
     await _usersCollection.Find(_ => true).ToListAsync();
     
    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) =>
        await _usersCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);
}