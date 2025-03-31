using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentCarApi.data;
using RentCarApi.DTO;
using RentCarApi.models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentCarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public UserController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            var existingUser = await _context.Users.FindAsync(request.PhoneNumber);
            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }

            var user = new User
            {
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                Role = request.Role ?? "User"
            };
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (user == null)
            {
                return BadRequest("User not found!");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password");
            }

            var token = CreateToken(user);
            return Ok(new { token, user.Name, user.Surname, user.Role, user.PhoneNumber, user.Email });
        }

        [HttpGet("{phoneNumber}/favorite-cars")]
        public IActionResult GetFavoriteCars(string phoneNumber)
        {
            var user = _context.Users
                .Include(u => u.FavoriteCars)
                .ThenInclude(fc => fc.Car)
                .FirstOrDefault(u => u.PhoneNumber == phoneNumber);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var favoriteCars = user.FavoriteCars.Select(fc => fc.Car);
            return Ok(favoriteCars);
        }

        [HttpPost("{phoneNumber}/favorites/{carId}")]
        public async Task<ActionResult> AddToFavorites(string phoneNumber, int carId)
        {
            var user = await _context.Users.FindAsync(phoneNumber);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return NotFound("Car not found");
            }

            var favoriteCar = new FavouriteCar // Changed spelling
            {
                UserPhoneNumber = phoneNumber,
                CarId = carId
            };

            await _context.FavoriteCars.AddAsync(favoriteCar);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{phoneNumber}")]
        public async Task<ActionResult<User>> GetUser(string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["AppSettings:Token"] ?? throw new InvalidOperationException("Token key not configured")));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
}