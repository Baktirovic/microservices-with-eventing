using Microsoft.EntityFrameworkCore;
using Account.API.Data;
using Account.API.Models;
using Shared.Models.SeedData;

namespace Account.API.Services;

public class DatabaseSeeder
{
    private readonly AccountDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AccountDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Database already seeded. Skipping seeding process.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            var seedUsers = SeedDataProvider.GetSeedUsers();
            var users = CreateUsersFromSeedData(seedUsers);
            var persons = CreatePersonsFromSeedData(seedUsers);

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            for (int i = 0; i < persons.Count; i++)
            {
                persons[i].UserId = users[i].Id;
            }

            await _context.Persons.AddRangeAsync(persons);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully. Added {UserCount} users and {PersonCount} persons.", 
                users.Count, persons.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static List<User> CreateUsersFromSeedData(List<SeedUser> seedUsers)
    {
        var random = new Random();
        return seedUsers.Select((seedUser, index) => new User
        {
            Id = seedUser.Id,
            Username = seedUser.Username,
            Email = seedUser.Email,
            PasswordHash = $"hashed_password_{seedUser.Id}",
            Salt = $"salt_{seedUser.Id}",
            IsActive = random.NextDouble() > 0.1, // 90% active
            IsEmailVerified = random.NextDouble() > 0.2, // 80% verified
            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 31)),
            LastLoginAt = DateTime.UtcNow.AddDays(-random.Next(0, 7))
        }).ToList();
    }

    private static List<Person> CreatePersonsFromSeedData(List<SeedUser> seedUsers)
    {
        var random = new Random();
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
        var states = new[] { "NY", "CA", "IL", "TX", "AZ", "PA", "TX", "CA", "TX", "CA" };
        var postalCodes = new[] { "10001", "90210", "60601", "77001", "85001", "19101", "78201", "92101", "75201", "95101" };
        var genders = new[] { "Male", "Female" };
        var professions = new[] { "Software Engineer", "Marketing Manager", "Financial Analyst", "Registered Nurse", "Project Manager", "Graphic Designer", "Sales Representative", "Teacher", "Accountant", "Developer" };

        return seedUsers.Select((seedUser, index) => new Person
        {
            FirstName = seedUser.FirstName,
            LastName = seedUser.LastName,
            MiddleName = seedUser.MiddleName,
            PhoneNumber = $"555-{1000 + index:D4}",
            Address = $"{random.Next(100, 999)} {new[] { "Main", "Oak", "Pine", "Elm", "Maple", "Cedar", "Birch", "Spruce", "Willow", "Ash" }[random.Next(10)]} Street",
            City = cities[index % cities.Length],
            State = states[index % states.Length],
            PostalCode = postalCodes[index % postalCodes.Length],
            Country = "USA",
            DateOfBirth = new DateTime(random.Next(1980, 2000), random.Next(1, 13), random.Next(1, 29)),
            Gender = genders[random.Next(genders.Length)],
            Notes = $"{professions[random.Next(professions.Length)]} with {random.Next(1, 20)} years experience",
            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 31))
        }).ToList();
    }
}
