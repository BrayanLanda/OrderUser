using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.UseCases;

public class RegisterUseCase(IUserRepository userRepo, IJwtService jwtService)
{
    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request,
        CancellationToken ct = default)
    {
        if(await userRepo.ExistsByEmailAsync(request.Email, ct))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered ");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Email, passwordHash, request.FirstName, request.LastName);
        
        await userRepo.CreateAsync(user, ct);
        
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = RefreshToken.Create(user.Id);

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