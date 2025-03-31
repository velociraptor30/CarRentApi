using System.Text.Json.Serialization;

using System.ComponentModel.DataAnnotations.Schema;

namespace RentCarApi.models
{
    public class Message
    {
        public int Id { get; set; }
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsSent { get; set; }

        [ForeignKey("UserPhoneNumber")]
        [JsonIgnore]
        public User User { get; set; }
        public string UserPhoneNumber { get; set; }
    }
}