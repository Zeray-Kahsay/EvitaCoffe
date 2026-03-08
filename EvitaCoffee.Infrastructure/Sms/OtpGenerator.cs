using System;
using System.Security.Cryptography;

namespace EvitaCoffee.Infrastructure.Sms;

public static class OtpGenerator
{
    public static string Generate()
    {
        return RandomNumberGenerator.GetInt32(100000,999999).ToString();
    }
}
