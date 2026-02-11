using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Domain.Entities;
using Finance.Api.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Finance.Infrastructure.Persistence;

namespace Finance.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", Register).WithName("Register");
        group.MapPost("/login", Login).WithName("Login");
    }

    private static async Task<IResult> Register(CreateUserRequest request, IAccountRepository accountRepo, ICategoryRepository categoryRepo, ITransactionRepository txRepo, AppDbContext db)
    {
        // simplistic: check existing username
        if (db.Set<User>().Any(u => u.Username == request.Username))
            return Results.BadRequest(new { error = "Username already exists" });

        var salt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: request.Password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);

        var hash = Convert.ToBase64String(hashBytes);
        var saltStr = Convert.ToBase64String(salt);

        var user = new User(request.Username, hash, saltStr); // User creation
        db.Set<User>().Add(user);
        await db.SaveChangesAsync();

        // create default account and categories for the user
        var account = new Account("Conta Principal", 0m, user.Id);
        await accountRepo.AddAsync(account);

        var c1 = new Category("Alimentação", user.Id);
        var c2 = new Category("Moradia", user.Id);
        await categoryRepo.AddAsync(c1);
        await categoryRepo.AddAsync(c2);

        return Results.Ok(new { message = "User registered" });
    }

    private static async Task<IResult> Login(LoginRequest request, AppDbContext db, JwtService jwt)
    {
        var user = db.Set<User>().FirstOrDefault(u => u.Username == request.Username);
        if (user == null) return Results.BadRequest(new { error = "Invalid credentials" });

        // Verify password using stored salt and same KDF
        try
        {
            var salt = Convert.FromBase64String(user.PasswordSalt);
            var hashBytes = KeyDerivation.Pbkdf2(
                password: request.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            var hash = Convert.ToBase64String(hashBytes);
            if (hash != user.PasswordHash)
                return Results.BadRequest(new { error = "Invalid credentials" });
        }
        catch
        {
            return Results.BadRequest(new { error = "Invalid credentials" });
        }

        var token = jwt.GenerateToken(user);
        return Results.Ok(new { token });
    }
}
