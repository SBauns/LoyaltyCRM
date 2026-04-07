using Bogus;
using Microsoft.AspNetCore.Identity;
using PapasCRM_API.Entities;
using PapasCRM_API.Models;

public static class ApplicationUserFactory
{
    private static readonly Faker<ApplicationUserEntity> Faker = new Faker<ApplicationUserEntity>()
        .RuleFor(u => u.UserName, f => f.Internet.UserName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.EmailConfirmed, f => true)
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());

    public static ApplicationUserEntity Create(Action<ApplicationUserEntity>? customizer = null)
    {
        var user = Faker.Generate();
        customizer?.Invoke(user); // Apply customizations if provided
        return user;
    }

    public static List<ApplicationUserEntity> CreateMany(int count, Action<ApplicationUserEntity>? customizer = null)
    {
        var users = new List<ApplicationUserEntity>();
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
