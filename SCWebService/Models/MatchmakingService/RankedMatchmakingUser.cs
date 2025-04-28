using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SCWebService.Models.MatchmakingService
{
    public class RankedMatchmakingUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public required int UserMMR { get; set; }
        public required string UserName { get; set; }
        public required string JoinCode { get; set; }   
    }
}
