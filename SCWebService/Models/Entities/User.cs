using System.Numerics;

namespace SCWebService.Models.Entities
{
    public class User
    {
        public Guid ID { get; set; }

        public required string Username { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required int Wins { get; set; }

        public required int Losses { get; set; }

        public required int MMR { get; set; }

        public required DateTime CreationDate { get; set; }
    }
}
