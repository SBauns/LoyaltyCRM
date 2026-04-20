using Bogus;
using LoyaltyCRM.Domain.Models;

public static class ApplicationUserFactory
{
    private static readonly Faker<ApplicationUser> Faker = new Faker<ApplicationUser>()
        .RuleFor(u => u.UserName, f => f.Internet.UserName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.EmailConfirmed, f => true)
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());

    public static ApplicationUser Create(Action<ApplicationUser>? customizer = null)
    {
        var user = Faker.Generate();
        customizer?.Invoke(user); // Apply customizations if provided
        return user;
    }

    public static List<ApplicationUser> CreateMany(int count, Action<ApplicationUser>? customizer = null)
    {
        var users = new List<ApplicationUser>();
        var usedEmails = new HashSet<string>();

        while (users.Count < count)
        {
            var user = Faker.Generate();
            
            // Ensure unique email
            if (!usedEmails.Add(user.Email))
                continue; // Skip duplicate

            customizer?.Invoke(user);
            users.Add(user);
        }

        return users;
    }
}
