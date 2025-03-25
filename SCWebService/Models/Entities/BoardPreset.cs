namespace SCWebService.Models.Entities
{
    public class BoardPreset
    {
        public required string UserID { get; set; }
        public required List<int> XVals { get; set; }
        public required List<int> YVals { get; set; }
        public required List<int> PieceTypes { get; set; }
    }
}