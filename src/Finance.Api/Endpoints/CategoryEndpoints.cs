using System.Security.Claims;
using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Application.UseCases;

namespace Finance.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapGet("/", GetAllCategories)
            .WithName("GetAllCategories")
            .WithSummary("Lista todas as categorias do usuário autenticado")
            .Produces<List<Finance.Domain.Entities.Category>>()
            .Produces(401);

        group.MapGet("/by-user/{ownerUserId:int}", GetCategoriesByOwnerUser)
            .WithName("GetCategoriesByOwnerUser")
            .WithSummary("Lista categorias vinculadas a um usuário (usa sempre o usuário da sessão)")
            .Produces<List<Finance.Domain.Entities.Category>>()
            .Produces(401)
            .Produces(403);

        group.MapGet("/{id:int}", GetCategoryById)
            .WithName("GetCategoryById")
            .WithSummary("Busca uma categoria por ID")
            .Produces<Finance.Domain.Entities.Category>()
            .Produces(401)
            .Produces(404);

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .WithSummary("Cria uma nova categoria")
            .Produces<Finance.Domain.Entities.Category>()
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapPut("/{id:int}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithSummary("Atualiza uma categoria existente")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        group.MapDelete("/{id:int}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithSummary("Exclui uma categoria")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);
    }

    private static async Task<IResult> GetAllCategories(HttpContext httpContext, ICategoryRepository repository)
    {
        var userId = GetAuthenticatedUserId(httpContext);
        if (!userId.HasValue)
            return Results.Unauthorized();

        var categories = await repository.GetByOwnerUserIdAsync(userId.Value);
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoriesByOwnerUser(
        HttpContext httpContext,
        int ownerUserId,
        GetCategoriesByOwnerUserUseCase useCase)
    {
        var userId = GetAuthenticatedUserId(httpContext);
        if (!userId.HasValue)
            return Results.Unauthorized();

        if (ownerUserId != userId.Value)
            return Results.Forbid();

        try
        {
            var categories = await useCase.ExecuteAsync(userId.Value);
            return Results.Ok(categories);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetCategoryById(HttpContext httpContext, int id, ICategoryRepository repository)
    {
        var userId = GetAuthenticatedUserId(httpContext);
        if (!userId.HasValue)
            return Results.Unauthorized();

        var category = await repository.GetByIdAsync(id);
        if (category == null) return Results.NotFound();
        if (category.OwnerUserId != userId.Value) return Results.Forbid();

        return Results.Ok(category);
    }

    private static async Task<IResult> CreateCategory(
        HttpContext httpContext,
        CreateCategoryUseCase useCase,
        CreateCategoryRequest request)
    {
        try
        {
            var userId = GetAuthenticatedUserId(httpContext);
            if (!userId.HasValue)
                return Results.Unauthorized();

            if (request.OwnerUserId.HasValue && request.OwnerUserId.Value != userId.Value)
                return Results.Forbid();

            var category = await useCase.ExecuteAsync(request.Name, userId.Value, request.Type, userId.Value);
            return Results.Ok(category);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCategory(
        HttpContext httpContext,
        int id,
        UpdateCategoryUseCase useCase,
        UpdateCategoryRequest request)
    {
        try
        {
            var userId = GetAuthenticatedUserId(httpContext);
            if (!userId.HasValue)
                return Results.Unauthorized();

            await useCase.ExecuteAsync(id, request.Name, userId.Value);
            return Results.Ok(new { message = "Category updated successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCategory(
        HttpContext httpContext,
        int id,
        DeleteCategoryUseCase useCase)
    {
        try
        {
            var userId = GetAuthenticatedUserId(httpContext);
            if (!userId.HasValue)
                return Results.Unauthorized();

            await useCase.ExecuteAsync(id, userId.Value);
            return Results.Ok(new { message = "Category deleted successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static int? GetAuthenticatedUserId(HttpContext httpContext)
    {
        var userClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirst("nameid")
            ?? httpContext.User.FindFirst("sub");

        if (userClaim is null)
            return null;

        return int.TryParse(userClaim.Value, out var userId)
            ? userId
            : null;
    }
}
