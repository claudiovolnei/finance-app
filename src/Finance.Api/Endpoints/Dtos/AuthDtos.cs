namespace Finance.Api.Endpoints.Dtos;

public record CreateUserRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);
