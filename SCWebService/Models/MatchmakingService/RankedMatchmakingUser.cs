namespace SCWebService.Models.MatchmakingService
{
    public class RankedMatchmakingUser
    {
        public required int UserMMR { get; set; }
        public required string UserName { get; set; }
        public required string JoinCode { get; set; }   
        public DateTime CreatedAt { get; set; }
    }
}
