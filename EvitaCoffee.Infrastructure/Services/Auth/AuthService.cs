using EvitaCoffee.Application.Abstractions;
using EvitaCoffee.Application.Common;
using EvitaCoffee.Application.Models;
using EvitaCoffee.Application.Services.Auth;
using EvitaCoffee.Contracts.Auth;
using EvitaCoffee.Contracts.Common;
using EvitaCoffee.Domain.Exceptions;
using EvitaCoffee.Infrastructure.Identity;
using EvitaCoffee.Infrastructure.Sms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EvitaCoffee.Infrastructure.Services.Auth;

public class AuthService(
    IRefreshTokenStore store,
    UserManager<AppUser> userManager,
    IJwtService jwt,
    IOtpStore otpStore,
    ISmsSender smsSender
    ) : IAuthService
{
    public async Task<Result<bool>> RegisterAsync(RegisterRequest request)
    {
        var normalizedPhone = PhoneNormalizer.Normalize(request.PhoneNumber);

        var existing = await userManager.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == normalizedPhone);

        if (existing is not null)
            return Result<bool>.Success(true);

        var user = new AppUser
        {
            UserName = normalizedPhone,
            PhoneNumber = normalizedPhone,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new DomainException("Registration failed");

        var otp = OtpGenerator.Generate();

        await otpStore.StoreAsync(normalizedPhone, otp);

        await smsSender.SendsAsync(normalizedPhone,
          $"Your Evita Coffee verification code is {otp}");

        return Result<bool>.Success(true);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var normalizePhone = PhoneNormalizer.Normalize(request.PhoneNumber);

        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == normalizePhone);

        await Task.Delay(Random.Shared.Next(200, 600));

        if (user is null)
            return Result<AuthResponse>.Failure(new ApiErrorResponse
            {
                Code = "InvalidCredentials",
                Message = "Invalid Credentials"
            });

        if (!user.PhoneNumberConfirmed)
            return Result<AuthResponse>.Failure(new ApiErrorResponse
            {
                Code = "PhoneNotConfirmed",
                Message = "Phone number not verified"
            });

        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);

        if (!validPassword)
            return Result<AuthResponse>.Failure(new ApiErrorResponse
            {
                Code = "InvalidCredentials",
                Message = "Invalid Credentials"
            });

        // If credentials are correct, issue access + refresh tokens using same rotation logic as RefreshAsync
        var userId = user.Id;
        var newTokenId = Guid.NewGuid().ToString();
        var combined = $"{userId}.{newTokenId}";
        var newHashed = RefreshTokenGenerator.Hash(combined);

        // persist the refresh token record for 7 days
        await store.StoreAsync(userId, newTokenId, newHashed, TimeSpan.FromDays(7));

        var authUser = new AuthUser(user.Id, user.PhoneNumber ?? string.Empty, user.FullName);
        var access = jwt.GenerateAccessToken(authUser);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = access,
            RefreshToken = combined,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });

    }

    public async Task<Result<AuthResponse>> RefreshAsync(string refreshToken)
    {
        var parts = refreshToken.Split('.');
        if (parts.Length != 2)
            return Result<AuthResponse>.Failure(new ApiErrorResponse
            {
                Code = "InvalidToken",
                Message = "Malformed refresh token"
            });

        var userId = Guid.Parse(parts[0]);
        var tokenId = parts[1];

        var hashed = RefreshTokenGenerator.Hash(refreshToken);

        var isValid = await store.ValidateAndDeleteAsync(userId, tokenId, hashed);

        if (!isValid)
            return Result<AuthResponse>.Failure(new ApiErrorResponse
            {
                Code = "InvalidToken",
                Message = "Invalid refresh token"
            });


        await store.RemoveAsync(userId, tokenId);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<AuthResponse>.Failure(new ApiErrorResponse
            {
                Code = "UserNotFound",
                Message = "User not found"
            });

        //var newRefreshRaw = RefreshTokenGenerator.GenerateRefreshToken();
        var newTokenId = Guid.NewGuid().ToString();

        var combined = $"{userId}.{newTokenId}";
        var newHashed = RefreshTokenGenerator.Hash(combined);

        await store.StoreAsync(userId, newTokenId, newHashed, TimeSpan.FromDays(7));

        var authUser = new AuthUser(user.Id, user.PhoneNumber ?? string.Empty, user.FullName);

        var access = jwt.GenerateAccessToken(authUser);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = access,
            RefreshToken = combined,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });
    }

    public async Task<Result<bool>> LogoutAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> VerifyPhoneAsync(VerifyPhoneRequest request)
    {
        var normalizedPhone = PhoneNormalizer.Normalize(request.PhoneNumber);
        var validation = await otpStore.ValidateAsync(normalizedPhone, request.Code);

        if (!validation.Success)
            return Result<bool>.Failure(new ApiErrorResponse
            {
                Code = "InvalidOtp",
                Message = validation.Error!
            });

        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == normalizedPhone)
            ?? throw new DomainException("User not found");

        user.PhoneNumberConfirmed = true;
        await userManager.UpdateAsync(user);

        return Result<bool>.Success(true);

    }


}
