using System;
using Microsoft.AspNetCore.Identity;

namespace EvitaCoffee.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string  FullName  { get; set; } = string.Empty; 
    public int  LoyaltyPoints  { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public void AddLoyalityPoints(int points)
    {
        LoyaltyPoints += points;
    }

    // UserName = PhoneNumber
}
