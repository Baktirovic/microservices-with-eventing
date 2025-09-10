using Microsoft.EntityFrameworkCore;
using Account.API.Data;
using Account.API.Models;

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

            var users = CreateSeedUsers();
            var persons = CreateSeedPersons();

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

    private static List<User> CreateSeedUsers()
    {
        return new List<User>
        {
            new User
            {
                Username = "john_doe",
                Email = "john.doe@email.com",
                PasswordHash = "hashed_password_1",
                Salt = "salt_1",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Username = "jane_smith",
                Email = "jane.smith@email.com",
                PasswordHash = "hashed_password_2",
                Salt = "salt_2",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                LastLoginAt = DateTime.UtcNow.AddDays(-2)
            },
            new User
            {
                Username = "mike_wilson",
                Email = "mike.wilson@email.com",
                PasswordHash = "hashed_password_3",
                Salt = "salt_3",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                LastLoginAt = DateTime.UtcNow.AddDays(-5)
            },
            new User
            {
                Username = "sarah_jones",
                Email = "sarah.jones@email.com",
                PasswordHash = "hashed_password_4",
                Salt = "salt_4",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Username = "david_brown",
                Email = "david.brown@email.com",
                PasswordHash = "hashed_password_5",
                Salt = "salt_5",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastLoginAt = DateTime.UtcNow.AddDays(-3)
            },
            new User
            {
                Username = "lisa_garcia",
                Email = "lisa.garcia@email.com",
                PasswordHash = "hashed_password_6",
                Salt = "salt_6",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Username = "robert_miller",
                Email = "robert.miller@email.com",
                PasswordHash = "hashed_password_7",
                Salt = "salt_7",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastLoginAt = DateTime.UtcNow.AddDays(-7)
            },
            new User
            {
                Username = "emily_davis",
                Email = "emily.davis@email.com",
                PasswordHash = "hashed_password_8",
                Salt = "salt_8",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Username = "chris_anderson",
                Email = "chris.anderson@email.com",
                PasswordHash = "hashed_password_9",
                Salt = "salt_9",
                IsActive = false,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                LastLoginAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Username = "amanda_taylor",
                Email = "amanda.taylor@email.com",
                PasswordHash = "hashed_password_10",
                Salt = "salt_10",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastLoginAt = DateTime.UtcNow.AddHours(-2)
            }
        };
    }

    private static List<Person> CreateSeedPersons()
    {
        return new List<Person>
        {
            new Person
            {
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "Michael",
                PhoneNumber = "555-0101",
                Address = "123 Main Street",
                City = "New York",
                State = "NY",
                PostalCode = "10001",
                Country = "USA",
                DateOfBirth = new DateTime(1985, 3, 15),
                Gender = "Male",
                Notes = "Software Engineer with 10+ years experience",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Person
            {
                FirstName = "Jane",
                LastName = "Smith",
                MiddleName = "Elizabeth",
                PhoneNumber = "555-0102",
                Address = "456 Oak Avenue",
                City = "Los Angeles",
                State = "CA",
                PostalCode = "90210",
                Country = "USA",
                DateOfBirth = new DateTime(1990, 7, 22),
                Gender = "Female",
                Notes = "Marketing Manager specializing in digital campaigns",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Person
            {
                FirstName = "Mike",
                LastName = "Wilson",
                MiddleName = "James",
                PhoneNumber = "555-0103",
                Address = "789 Pine Road",
                City = "Chicago",
                State = "IL",
                PostalCode = "60601",
                Country = "USA",
                DateOfBirth = new DateTime(1988, 11, 8),
                Gender = "Male",
                Notes = "Financial Analyst with CFA certification",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Person
            {
                FirstName = "Sarah",
                LastName = "Jones",
                MiddleName = "Marie",
                PhoneNumber = "555-0104",
                Address = "321 Elm Street",
                City = "Houston",
                State = "TX",
                PostalCode = "77001",
                Country = "USA",
                DateOfBirth = new DateTime(1992, 4, 12),
                Gender = "Female",
                Notes = "Registered Nurse working in emergency care",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Person
            {
                FirstName = "David",
                LastName = "Brown",
                MiddleName = "Robert",
                PhoneNumber = "555-0105",
                Address = "654 Maple Drive",
                City = "Phoenix",
                State = "AZ",
                PostalCode = "85001",
                Country = "USA",
                DateOfBirth = new DateTime(1987, 9, 3),
                Gender = "Male",
                Notes = "Project Manager in construction industry",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Person
            {
                FirstName = "Lisa",
                LastName = "Garcia",
                MiddleName = "Isabella",
                PhoneNumber = "555-0106",
                Address = "987 Cedar Lane",
                City = "Philadelphia",
                State = "PA",
                PostalCode = "19101",
                Country = "USA",
                DateOfBirth = new DateTime(1991, 12, 18),
                Gender = "Female",
                Notes = "Graphic Designer with expertise in UI/UX",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Person
            {
                FirstName = "Robert",
                LastName = "Miller",
                MiddleName = "Thomas",
                PhoneNumber = "555-0107",
                Address = "147 Birch Court",
                City = "San Antonio",
                State = "TX",
                PostalCode = "78201",
                Country = "USA",
                DateOfBirth = new DateTime(1986, 6, 25),
                Gender = "Male",
                Notes = "Sales Representative in pharmaceutical industry",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Person
            {
                FirstName = "Emily",
                LastName = "Davis",
                MiddleName = "Grace",
                PhoneNumber = "555-0108",
                Address = "258 Spruce Street",
                City = "San Diego",
                State = "CA",
                PostalCode = "92101",
                Country = "USA",
                DateOfBirth = new DateTime(1993, 1, 14),
                Gender = "Female",
                Notes = "Elementary school teacher with 5 years experience",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Person
            {
                FirstName = "Chris",
                LastName = "Anderson",
                MiddleName = "Lee",
                PhoneNumber = "555-0109",
                Address = "369 Willow Way",
                City = "Dallas",
                State = "TX",
                PostalCode = "75201",
                Country = "USA",
                DateOfBirth = new DateTime(1984, 8, 30),
                Gender = "Male",
                Notes = "Accountant with CPA license (inactive account)",
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Person
            {
                FirstName = "Amanda",
                LastName = "Taylor",
                MiddleName = "Nicole",
                PhoneNumber = "555-0110",
                Address = "741 Ash Boulevard",
                City = "San Jose",
                State = "CA",
                PostalCode = "95101",
                Country = "USA",
                DateOfBirth = new DateTime(1995, 5, 7),
                Gender = "Female",
                Notes = "Recent graduate in Computer Science",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
    }
}
