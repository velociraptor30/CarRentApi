using System.Text.Json.Serialization;

namespace RentCarApi.models
{
    public class FavouriteCar
    {
        public int Id { get; set; }
        public string UserPhoneNumber { get; set; }
        public int CarId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public Car Car { get; set; }
    }
}