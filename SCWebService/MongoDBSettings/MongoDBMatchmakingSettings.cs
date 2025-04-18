namespace SCWebService.MongoDBSettings
{
    public class MongoDBMatchmakingSettings
    {
        public string ConnectionURI { get; set; } = Environment.GetEnvironmentVariable("ConnectionURI")!;
        public string DatabaseName { get; set; } = Environment.GetEnvironmentVariable("DatabaseName")!;
        public string CollectionName { get; set; } = Environment.GetEnvironmentVariable("MatchmakingUserCollectionName")!;
    }
}
