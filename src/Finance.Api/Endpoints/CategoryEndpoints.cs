using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Application.UseCases;

namespace Finance.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories")
            .WithTags("Categories");

        group.MapGet("/", GetAllCategories)
            .WithName("GetAllCategories")
            .WithSummary("Lista todas as categorias")
            .Produces<List<Finance.Domain.Entities.Category>>();

        group.MapGet("/{id:guid}", GetCategoryById)
            .WithName("GetCategoryById")
            .WithSummary("Busca uma categoria por ID")
            .Produces<Finance.Domain.Entities.Category>()
            .Produces(404);

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .WithSummary("Cria uma nova categoria")
            .Produces<Finance.Domain.Entities.Category>()
            .Produces(400);

        group.MapPut("/{id:guid}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithSummary("Atualiza uma categoria existente")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:guid}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithSummary("Exclui uma categoria")
            .Produces(200)
            .Produces(400)
            .Produces(404);
    }

    private static async Task<IResult> GetAllCategories(ICategoryRepository repository)
    {
        var categories = await repository.GetAllAsync();
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoryById(Guid id, ICategoryRepository repository)
    {
        var category = await repository.GetByIdAsync(id);
        return category != null ? Results.Ok(category) : Results.NotFound();
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryUseCase useCase,
        CreateCategoryRequest request)
    {
        try
        {
            var category = await useCase.ExecuteAsync(request.Name);
            return Results.Ok(category);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCategory(
        Guid id,
        UpdateCategoryUseCase useCase,
        UpdateCategoryRequest request)
    {
        try
        {
            await useCase.ExecuteAsync(id, request.Name);
            return Results.Ok(new { message = "Category updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        DeleteCategoryUseCase useCase)
    {
        try
        {
            await useCase.ExecuteAsync(id);
            return Results.Ok(new { message = "Category deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
