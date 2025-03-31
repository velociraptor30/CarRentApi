using System; // Required for basic types

namespace RentCarApi.DTO
{
    public class LoginDto
    {
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}