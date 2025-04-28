using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SCWebService.Models.MatchmakingService;
using SCWebService.MongoDBSettings;
namespace SCWebService.Services
{
    public class RankedMatchmakingService
    {
        private readonly IMongoCollection<RankedMatchmakingUser> _matchmakingCollection;
        public RankedMatchmakingService(IOptions<RankedMatchmakingSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _matchmakingCollection = database.GetCollection<RankedMatchmakingUser>(mongoDBSettings.Value.CollectionName);
        }
        public async Task<RankedMatchmakingUser?> FindValidHostAsync(string userName, int userMMR)
        {
            return await _matchmakingCollection.Find(x => x.UserName != userName).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(RankedMatchmakingUser newUser)
        {
            await _matchmakingCollection.InsertOneAsync(newUser);
        }

        public async Task<bool> TryRemoveFromQueue(string username)
        {
            DeleteResult result = await _matchmakingCollection.DeleteOneAsync(x => x.UserName == username);
            return result.IsAcknowledged;

        }
    }
}
