using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SCWebService.Models.UserService
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string userName { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public int userMMR { get; set; } = 800;
        public int wins { get; set; }
        public int losses { get; set; }
        public int draws { get; set; }
        public BoardPreset[]? boardPresets { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
