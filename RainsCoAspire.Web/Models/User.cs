namespace RainsCoAspire.Web.Models;

public record User(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    string Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// Token is the API's JWT, sent as a bearer token until ExpiresAt.
public record LoginResult(string Token, DateTimeOffset ExpiresAt, User User);
