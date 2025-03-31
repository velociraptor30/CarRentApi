using System.Text.Json.Serialization;

namespace RentCarApi.models
{
    public class Rental
    {
        public int Id { get; set; }
        public string UserPhoneNumber { get; set; }
        public int CarId { get; set; }
        public string City { get; set; }
        public int Days { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime StartDate { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public Car Car { get; set; }
    }
}