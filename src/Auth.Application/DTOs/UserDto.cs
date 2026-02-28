namespace Auth.Application.DTOs;

public record UserDto(
    Guid ExternalId,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    List<string> Roles
);