using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace RentCarApi.models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string City { get; set; }
        
        // Image storage as a list
        public List<string> ImageUrls { get; set; } = new List<string>();
        [NotMapped]
        public List<IFormFile> Images { get; set; } // Temporary for uploads

        [ForeignKey("OwnerPhoneNumber")]
        [JsonIgnore]
        public User Owner { get; set; }
        public string OwnerPhoneNumber { get; set; }

        [JsonIgnore]
        public List<FavouriteCar> FavoritedByUsers { get; set; }

        [JsonIgnore]
        public List<Rental> Rentals { get; set; }
    }
}