using System;

namespace EvitaCoffee.Application.Abstractions;

public interface ISmsSender
{
    Task SendsAsync(string phoneNumber, string message);
}
