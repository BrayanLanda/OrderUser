using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.UseCases;

public class RefreshTokenUseCase(
    IRefreshTokenRepository refreshTokenRepo,
    IUserRepository userRepo,
    IJwtService jwtService)
{
    public async Task<AuthResponse> ExecuteAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var token = await refreshTokenRepo.GetByTokenAsync(request.RefreshToken, ct)
                    ?? throw new UnauthorizedAccessException("Invalid token");

        if (!token.IsValid)
            throw new UnauthorizedAccessException("Token expired or revoked");

        var user = await userRepo.GetByIdAsync(token.UserId, ct)
                   ?? throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("The account is disabled");

        token.Revoke();
        await refreshTokenRepo.UpdateAsync(token, ct);

        var newRefreshToken = RefreshToken.Create(user.Id);
        await refreshTokenRepo.CreateAsync(newRefreshToken, ct);

        var accessToken = jwtService.GenerateAccessToken(user);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken.Token,
            ExpiresAt: jwtService.GetAccessTokenExpiration(),
            User: new UserDto(user.ExternalId, user.Email, user.FirstName, user.LastName, user.IsActive, user.Roles)
        );
    }
}