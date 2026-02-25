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
            .WithSummary("Lista todas as categorias")
            .Produces<List<Finance.Domain.Entities.Category>>();

        group.MapGet("/{id:int}", GetCategoryById)
            .WithName("GetCategoryById")
            .WithSummary("Busca uma categoria por ID")
            .Produces<Finance.Domain.Entities.Category>()
            .Produces(404);

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .WithSummary("Cria uma nova categoria")
            .Produces<Finance.Domain.Entities.Category>()
            .Produces(400);

        group.MapPut("/{id:int}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithSummary("Atualiza uma categoria existente")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:int}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithSummary("Exclui uma categoria")
            .Produces(200)
            .Produces(400)
            .Produces(404);
    }

    private static async Task<IResult> GetAllCategories(HttpContext httpContext, ICategoryRepository repository)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        int userId = 0;
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        var categories = await repository.GetByUserIdAsync(userId);
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoryById(HttpContext httpContext, int id, ICategoryRepository repository)
    {
        var category = await repository.GetByIdAsync(id);
        if (category == null) return Results.NotFound();
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        int userId = 0;
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        if (category.UserId != userId) return Results.Forbid();

        return Results.Ok(category);
    }

    private static async Task<IResult> CreateCategory(
        HttpContext httpContext,
        CreateCategoryUseCase useCase,
        CreateCategoryRequest request)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
                userId = parsed;

            var category = await useCase.ExecuteAsync(request.Name, userId);
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
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
                userId = parsed;

            await useCase.ExecuteAsync(id, request.Name, userId);
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
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
                userId = parsed;

            await useCase.ExecuteAsync(id, userId);
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
}
