
namespace SCWebService.Models.MatchmakingService
{
    public class MatchSubmission
    {
        public required string Token { get; set; }
        public required bool Victory { get; set; }
        public required bool Ranked { get; set; }
        public required int LocalMMR { get; set; }
        public required int OppositionMMR { get; set; }

        public static int CalculateRewards(bool victory) 
        {
            if (victory)
            {
                return 450;
            }
            else
            {
                return 200;
            }
        }

        internal static int CalculateMMRChange(int localMMR, int oppositionMMR, bool victory)
        {

            // K-factor determines the maximum possible change
            const int kFactor = 32;

            // Calculate expected score for player1
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (localMMR - oppositionMMR) / 400.0));

            // Actual score is 1 if won, 0 if lost
            double actualScore = victory ? 1.0 : 0.0;

            // Calculate MMR change
            int mmrChange = (int)Math.Round(kFactor * (actualScore - expectedScore));

            return mmrChange;
        }
    }
}
