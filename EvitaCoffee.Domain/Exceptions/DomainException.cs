using System;

namespace EvitaCoffee.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message){}
    
}
