namespace SCWebService.Models
{
    public class MongoDBUserSettings
    {
        public string ConnectionURI { get; set; } = Environment.GetEnvironmentVariable("ConnectionURI")!;
        public string DatabaseName { get; set; } = Environment.GetEnvironmentVariable("UserDatabaseName")!;
        public string CollectionName { get; set; } = Environment.GetEnvironmentVariable("UserCollectionName")!;
    }
}
