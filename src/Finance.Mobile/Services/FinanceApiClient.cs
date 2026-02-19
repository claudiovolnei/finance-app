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
        var resp = await _httpClient.GetAsync(Url($"dashboard/summary"));
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
    public async Task<List<TransactionDto>> GetTransactionsAsync()
    {
        var resp = await _httpClient.GetAsync(Url("transactions"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        resp.EnsureSuccessStatusCode();
        var transactions = await resp.Content.ReadFromJsonAsync<List<TransactionDto>>();
        return transactions ?? new List<TransactionDto>();
    }

    public async Task<List<TransactionDto>> GetTransactionsAsync(int? year, int? month)
    {
        var url = "transactions";
        var query = new List<string>();
        if (year.HasValue) query.Add($"year={year.Value}");
        if (month.HasValue) query.Add($"month={month.Value}");
        if (query.Any()) url += "?" + string.Join("&", query);
        var resp = await _httpClient.GetAsync(Url(url));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        resp.EnsureSuccessStatusCode();
        var transactions = await resp.Content.ReadFromJsonAsync<List<TransactionDto>>();
        return transactions ?? new List<TransactionDto>();
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
    {
        var resp = await _httpClient.GetAsync(Url($"transactions/{id}"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TransactionDto>();
    }

    public async Task CreateTransactionAsync(
        int accountId,
        int categoryId,
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
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTransactionAsync(
        int id,
        int accountId,
        int categoryId,
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
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(Url($"transactions/{id}"));
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        response.EnsureSuccessStatusCode();
    }

    // ========== CATEGORIES ==========
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var resp = await _httpClient.GetAsync(Url("categories"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        resp.EnsureSuccessStatusCode();
        var categories = await resp.Content.ReadFromJsonAsync<List<Category>>();
        return categories ?? new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        var resp = await _httpClient.GetAsync(Url($"categories/{id}"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Category>();
    }

    public async Task<Category> CreateCategoryAsync(string name)
    {
        var request = new CreateCategoryRequest(name);
        var response = await _httpClient.PostAsJsonAsync(Url("categories"), request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>() ?? throw new Exception("Failed to create category");
    }

    public async Task UpdateCategoryAsync(int id, string name)
    {
        var request = new UpdateCategoryRequest(name);
        var response = await _httpClient.PutAsJsonAsync(Url($"categories/{id}"), request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(Url($"categories/{id}"));
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        response.EnsureSuccessStatusCode();
    }

    // ========== ACCOUNTS ==========
    public async Task<List<Account>> GetAccountsAsync()
    {
        var accounts = await _httpClient.GetFromJsonAsync<List<Account>>(Url("accounts"));
        return accounts ?? new List<Account>();
    }

    public async Task<Account?> GetAccountByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Account>(Url($"accounts/{id}"));
    }

    // Request DTOs
    private record CreateTransactionRequest(
        int AccountId,
        int CategoryId,
        decimal Amount,
        DateTime Date,
        string? Description,
        TransactionType Type);

    // dashboard DTOs are defined in src/Finance.Mobile/Services/DashboardDtos.cs

    private record UpdateTransactionRequest(
        int AccountId,
        int CategoryId,
        decimal Amount,
        DateTime Date,
        string? Description,
        TransactionType Type);

    private record CreateCategoryRequest(string Name);
    private record UpdateCategoryRequest(string Name);
}
