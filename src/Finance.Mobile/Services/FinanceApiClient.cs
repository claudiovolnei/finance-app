using System.Net.Http.Json;
using Finance.Domain.Entities;
using System.Net.Http.Headers;
using Finance.Mobile.Services;

namespace Finance.Mobile.Services;

public class FinanceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiBaseUrl _baseUrl;

    public FinanceApiClient(HttpClient httpClient, ApiBaseUrl baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    private string Url(string path) => _baseUrl.GetAbsoluteUrl(path);

    public void SetBearerToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = null;
        else
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync()
    {
        var resp = await _httpClient.GetAsync(Url("dashboard/summary"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return null;

        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        return dto;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var resp = await _httpClient.PostAsJsonAsync(Url("auth/login"), new { Username = username, Password = password });
        if (!resp.IsSuccessStatusCode) return null;
        var text = await resp.Content.ReadAsStringAsync();
        try
        {
            var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(text);
            if (obj != null && obj.TryGetValue("token", out var token)) return token;
        }
        catch
        {
        }
        return null;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        var resp = await _httpClient.PostAsJsonAsync(Url("auth/register"), new { Username = username, Password = password });
        return resp.IsSuccessStatusCode;
    }

    // ========== TRANSACTIONS ==========
    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        var transactions = await _httpClient.GetFromJsonAsync<List<Transaction>>(Url("transactions"));
        return transactions ?? new List<Transaction>();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Transaction>(Url($"transactions/{id}"));
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

        var response = await _httpClient.PostAsJsonAsync(Url("transactions"), request);
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

        var response = await _httpClient.PutAsJsonAsync(Url($"transactions/{id}"), request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync(Url($"transactions/{id}"));
        response.EnsureSuccessStatusCode();
    }

    // ========== CATEGORIES ==========
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = await _httpClient.GetFromJsonAsync<List<Category>>(Url("categories"));
        return categories ?? new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Category>(Url($"categories/{id}"));
    }

    public async Task<Category> CreateCategoryAsync(string name)
    {
        var request = new CreateCategoryRequest(name);
        var response = await _httpClient.PostAsJsonAsync(Url("categories"), request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>() ?? throw new Exception("Failed to create category");
    }

    public async Task UpdateCategoryAsync(Guid id, string name)
    {
        var request = new UpdateCategoryRequest(name);
        var response = await _httpClient.PutAsJsonAsync(Url($"categories/{id}"), request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync(Url($"categories/{id}"));
        response.EnsureSuccessStatusCode();
    }

    // ========== ACCOUNTS ==========
    public async Task<List<Account>> GetAccountsAsync()
    {
        var accounts = await _httpClient.GetFromJsonAsync<List<Account>>(Url("accounts"));
        return accounts ?? new List<Account>();
    }

    public async Task<Account?> GetAccountByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Account>(Url($"accounts/{id}"));
    }

    // Request DTOs
    private record CreateTransactionRequest(
        Guid AccountId,
        Guid CategoryId,
        decimal Amount,
        DateTime Date,
        string? Description,
        TransactionType Type);

    // dashboard DTOs are defined in src/Finance.Mobile/Services/DashboardDtos.cs

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
