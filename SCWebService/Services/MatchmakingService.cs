using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SCWebService.Models.MatchmakingService;
using SCWebService.MongoDBSettings;
namespace SCWebService.Services
{
    public class MatchmakingService
    {
        private readonly IMongoCollection<MatchmakingUser> _matchmakingCollection;
        public MatchmakingService(IOptions<MongoDBMatchmakingSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _matchmakingCollection = database.GetCollection<MatchmakingUser>(mongoDBSettings.Value.CollectionName);
        }
        public async Task<MatchmakingUser?> FindValidHostAsync(string userName, int userMMR)
        {
            return await _matchmakingCollection.Find(x => x.UserName != userName).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(MatchmakingUser newUser)
        {
            await _matchmakingCollection.InsertOneAsync(newUser);
        }

        public async Task<bool> TryRemoveFromQueue(MatchmakingUser userToRemove)
        {
            DeleteResult result = await _matchmakingCollection.DeleteOneAsync(x => x.UserName == userToRemove.UserName);
            return result.IsAcknowledged;

        }
    }
}
