using System;

namespace EvitaCoffee.Application.Models;

public record AuthUser(Guid Id, string PhoneNumber, string FullName);

