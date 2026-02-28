using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.UseCases;

public class LoginUseCase(
    IUserRepository userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IJwtService jwtService)
{
    public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepo.GetByEmailAsync(request.Email, ct)
                   ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("The account is disabled");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
        
        await refreshTokenRepo.RevokeAllForUserAsync(user.Id, ct);

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = RefreshToken.Create(user.Id);
        await refreshTokenRepo.CreateAsync(refreshToken, ct);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiresAt: jwtService.GetAccessTokenExpiration(),
            User: MapToDto(user)
        );
    }

    private static UserDto MapToDto(User user) => new(
        user.ExternalId, user.Email, user.FirstName, user.LastName, user.IsActive, user.Roles
    );
}