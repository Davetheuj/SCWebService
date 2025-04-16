using System.Text.Json.Serialization;

namespace SCWebService.Models
{
    [Serializable]
    public class BoardPreset
    {
        public SerializableVector3[]? pieces;
    }
}
