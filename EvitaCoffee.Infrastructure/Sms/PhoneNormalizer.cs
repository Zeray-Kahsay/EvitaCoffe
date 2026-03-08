using System;
using EvitaCoffee.Domain.Exceptions;
using PhoneNumbers;


namespace EvitaCoffee.Infrastructure.Sms;

public static class PhoneNormalizer
{
    public static string Normalize(string input)
    {
        var util = PhoneNumberUtil.GetInstance();
        var parsed = util.Parse(input, "NO");

        if (!util.IsValidNumber(parsed))
            throw new DomainException("Invalid phone number");
        
        return util.Format(parsed, PhoneNumberFormat.E164);
    }
}
