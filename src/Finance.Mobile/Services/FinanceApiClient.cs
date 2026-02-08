using System.Net.Http.Json;
using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

public class FinanceApiClient
{
    private readonly HttpClient _httpClient;

    public FinanceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ========== TRANSACTIONS ==========
    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        var transactions = await _httpClient.GetFromJsonAsync<List<Transaction>>("/transactions");
        return transactions ?? new List<Transaction>();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Transaction>($"/transactions/{id}");
    }

    public async Task CreateTransactionAsync(
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type)
    {
        var request = new CreateTransactionRequest(
            accountId,
            categoryId,
            amount,
            date,
            description,
            type);

        var response = await _httpClient.PostAsJsonAsync("/transactions", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTransactionAsync(
        Guid id,
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type)
    {
        var request = new UpdateTransactionRequest(
            accountId,
            categoryId,
            amount,
            date,
            description,
            type);

        var response = await _httpClient.PutAsJsonAsync($"/transactions/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/transactions/{id}");
        response.EnsureSuccessStatusCode();
    }

    // ========== CATEGORIES ==========
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = await _httpClient.GetFromJsonAsync<List<Category>>("/categories");
        return categories ?? new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Category>($"/categories/{id}");
    }

    public async Task<Category> CreateCategoryAsync(string name)
    {
        var request = new CreateCategoryRequest(name);
        var response = await _httpClient.PostAsJsonAsync("/categories", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>() ?? throw new Exception("Failed to create category");
    }

    public async Task UpdateCategoryAsync(Guid id, string name)
    {
        var request = new UpdateCategoryRequest(name);
        var response = await _httpClient.PutAsJsonAsync($"/categories/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/categories/{id}");
        response.EnsureSuccessStatusCode();
    }

    // ========== ACCOUNTS ==========
    public async Task<List<Account>> GetAccountsAsync()
    {
        var accounts = await _httpClient.GetFromJsonAsync<List<Account>>("/accounts");
        return accounts ?? new List<Account>();
    }

    public async Task<Account?> GetAccountByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Account>($"/accounts/{id}");
    }

    // Request DTOs
    private record CreateTransactionRequest(
        Guid AccountId,
        Guid CategoryId,
        decimal Amount,
        DateTime Date,
        string? Description,
        TransactionType Type);

    private record UpdateTransactionRequest(
        Guid AccountId,
        Guid CategoryId,
        decimal Amount,
        DateTime Date,
        string? Description,
        TransactionType Type);

    private record CreateCategoryRequest(string Name);
    private record UpdateCategoryRequest(string Name);
}
