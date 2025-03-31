using System.Text.Json.Serialization;

namespace RentCarApi.models
{
    public class User
    {
        public string PhoneNumber { get; set; } // Primary key
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        
        public string Role { get; set; }
        
        [JsonIgnore]
        public List<Car> OwnedCars { get; set; }

        [JsonIgnore]
        public List<FavouriteCar> FavoriteCars { get; set; }

        [JsonIgnore]
        public List<Rental> Rentals { get; set; }
        
        [JsonIgnore]
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}