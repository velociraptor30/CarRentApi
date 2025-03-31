using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarApi.data;
using RentCarApi.DTO; // AddCarDto
using RentCarApi.models; // Car, FavouriteCar, Rental, User
using RentCarApi.Repository.Abstract;

namespace RentCarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFileService _fileService;

        public CarController(DataContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/Car - Get 12 Random Cars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
            var cars = await _context.Cars
                .OrderBy(c => Guid.NewGuid())
                .Take(12)
                .ToListAsync();

            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(cars);
        }

        // GET: api/Car/paginated - Get Paginated Cars (returns list only)
        [HttpGet("paginated")]
        public async Task<ActionResult<IEnumerable<Car>>> GetPaginatedCars(int pageIndex = 1, int pageSize = 10)
        {
            if (pageIndex < 1)
            {
                return BadRequest("Invalid page index.");
            }

            var cars = await _context.Cars
                .OrderBy(c => c.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(cars);
        }

        // GET: api/Car/filter - Get Filtered Cars (returns list only)
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Car>>> FilterCars(int? seats, int? startYear, int? endYear, string? city, int pageIndex = 1, int pageSize = 10)
        {
            IQueryable<Car> query = _context.Cars;

            if (seats.HasValue)
            {
                query = query.Where(c => c.Seats == seats.Value);
            }

            if (startYear.HasValue)
            {
                query = query.Where(c => c.Year >= startYear.Value);
            }

            if (endYear.HasValue)
            {
                query = query.Where(c => c.Year <= endYear.Value);
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(c => c.City == city);
            }

            var cars = await query
                .OrderBy(c => c.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(cars);
        }

        // GET: api/Car/popular - Get 4 Random Popular Cars
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<Car>>> GetPopularCars()
        {
            var cars = await _context.Cars
                .OrderBy(c => Guid.NewGuid())
                .Take(4)
                .ToListAsync();

            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(cars);
        }

        // GET: api/Car/byPhone - Get Cars by Owner Phone Number
        [HttpGet("byPhone")]
        public async Task<ActionResult<IEnumerable<Car>>> GetCarsByOwner(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return BadRequest("Phone number is required.");
            }

            var cars = await _context.Cars
                .Where(c => c.OwnerPhoneNumber == phoneNumber)
                .ToListAsync();

            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(cars);
        }

        // GET: api/Car/cities - Get Distinct Cities
        [HttpGet("cities")]
        public async Task<ActionResult<IEnumerable<string>>> GetCities()
        {
            var cities = await _context.Cars
                .Select(c => c.City)
                .Distinct()
                .ToListAsync();

            if (cities == null || !cities.Any())
            {
                return NotFound();
            }

            return Ok(cities);
        }

        // GET: api/Car/5 - Get Car by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }

            return Ok(car);
        }

        // POST: api/Car - Create a New Car with Images
        [HttpPost]
        public async Task<ActionResult<Car>> PostCar([FromForm] AddCarDto carInput)
        {
            var car = new Car
            {
                Brand = carInput.Brand,
                Model = carInput.Model,
                Year = carInput.Year,
                PricePerDay = carInput.PricePerDay,
                Seats = carInput.Seats,
                Transmission = carInput.Transmission,
                City = carInput.City,
                OwnerPhoneNumber = HttpContext.User.Identity?.Name ?? carInput.OwnerPhoneNumber // Fallback if not authenticated
                // FuelCapacity is in AddCarDto but not in Car model
            };

            // Handle image uploads
            if (carInput.Images != null && carInput.Images.Any())
            {
                var imageUrls = await _fileService.SaveImages(carInput.Images);
                car.ImageUrls = imageUrls;
            }

            // Stack-like behavior: Remove oldest car if count >= 500
            int count = await _context.Cars.CountAsync();
            if (count >= 500)
            {
                var firstCar = await _context.Cars
                    .Include(c => c.FavoritedByUsers)
                    .Include(c => c.Rentals)
                    .FirstOrDefaultAsync();

                if (firstCar != null)
                {
                    _context.FavoriteCars.RemoveRange(firstCar.FavoritedByUsers); // Fixed to match DataContext
                    _context.Rentals.RemoveRange(firstCar.Rentals);
                    _context.Cars.Remove(firstCar);
                }
            }

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCar), new { id = car.Id }, car);
        }
    }
}