using System; // Required for basic types
using Microsoft.AspNetCore.Http; // For IFormFile

namespace RentCarApi.DTO
{

    public class AddCarDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string City { get; set; }
        public int FuelCapacity { get; set; }
        public string OwnerPhoneNumber { get; set; } // Added to match Car model
        public List<IFormFile> Images { get; set; } // For image uploads
    }
}