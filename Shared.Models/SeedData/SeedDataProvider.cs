using Shared.Models.Models;

namespace Shared.Models.SeedData;

public static class SeedDataProvider
{
    public static List<SeedUser> GetSeedUsers()
    {
        return new List<SeedUser>
        {
            new SeedUser
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Username = "john_doe",
                Email = "john.doe@email.com",
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "Michael"
            },
            new SeedUser
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Username = "jane_smith",
                Email = "jane.smith@email.com",
                FirstName = "Jane",
                LastName = "Smith",
                MiddleName = "Elizabeth"
            },
            new SeedUser
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Username = "mike_wilson",
                Email = "mike.wilson@email.com",
                FirstName = "Mike",
                LastName = "Wilson",
                MiddleName = "James"
            },
            new SeedUser
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Username = "sarah_jones",
                Email = "sarah.jones@email.com",
                FirstName = "Sarah",
                LastName = "Jones",
                MiddleName = "Marie"
            },
            new SeedUser
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Username = "david_brown",
                Email = "david.brown@email.com",
                FirstName = "David",
                LastName = "Brown",
                MiddleName = "Robert"
            },
            new SeedUser
            {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                Username = "lisa_garcia",
                Email = "lisa.garcia@email.com",
                FirstName = "Lisa",
                LastName = "Garcia",
                MiddleName = "Isabella"
            },
            new SeedUser
            {
                Id = new Guid("77777777-7777-7777-7777-777777777777"),
                Username = "robert_miller",
                Email = "robert.miller@email.com",
                FirstName = "Robert",
                LastName = "Miller",
                MiddleName = "Thomas"
            },
            new SeedUser
            {
                Id = new Guid("88888888-8888-8888-8888-888888888888"),
                Username = "emily_davis",
                Email = "emily.davis@email.com",
                FirstName = "Emily",
                LastName = "Davis",
                MiddleName = "Grace"
            },
            new SeedUser
            {
                Id = new Guid("99999999-9999-9999-9999-999999999999"),
                Username = "chris_anderson",
                Email = "chris.anderson@email.com",
                FirstName = "Chris",
                LastName = "Anderson",
                MiddleName = "Lee"
            },
            new SeedUser
            {
                Id = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                Username = "amanda_taylor",
                Email = "amanda.taylor@email.com",
                FirstName = "Amanda",
                LastName = "Taylor",
                MiddleName = "Nicole"
            }
        };
    }

    public static List<SeedAccount> GetSeedAccounts(List<SeedUser> users)
    {
        if (users == null || users.Count == 0)
            return new List<SeedAccount>();

        var accounts = new List<SeedAccount>();
        var random = new Random(42); // Fixed seed for consistent results

        // Create accounts for each user
        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            
            // Each user gets at least one checking account
            accounts.Add(new SeedAccount
            {
                UserId = user.Id,
                AccountNumber = $"ACC{(i + 1):D3}",
                AccountType = "Checking",
                Balance = (decimal)(random.NextDouble() * 10000 + 1000) // $1000-$11000
            });

            // Some users get additional savings accounts
            if (i % 2 == 0) // Every other user gets a savings account
            {
                accounts.Add(new SeedAccount
                {
                    UserId = user.Id,
                    AccountNumber = $"ACC{(i + 1):D3}S",
                    AccountType = "Savings",
                    Balance = (decimal)(random.NextDouble() * 20000 + 5000) // $5000-$25000
                });
            }
        }

        return accounts;
    }

    public static List<SeedAuditUser> GetSeedAuditUsers()
    {
        var users = GetSeedUsers();
        return users.Select(user => new SeedAuditUser
        {
            Id = new Guid($"B{user.Id.ToString().Substring(1)}"),
            ExternalId = user.Id.ToString(),
            Name = $"{user.FirstName} {user.LastName}",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        }).ToList();
    }
}
