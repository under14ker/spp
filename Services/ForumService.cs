using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForumClient.Models;

namespace ForumClient.Services;

public class ForumService
{
    private readonly HttpClient _httpClient;
    private string? _token;
    
    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);
    public string? CurrentUsername { get; private set; }
    
    public ForumService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (!string.IsNullOrEmpty(result?.Token))
                {
                    _token = result.Token;

                    // сохраняем токен в заголовках для всех будущих запросов
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token);

                    CurrentUsername = ExtractUsernameFromToken(_token);
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    public void Logout()
    {
        _token = null;
        CurrentUsername = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
    
    public async Task<List<TopicDto>> GetTopicsAsync()
    {
        var response = await _httpClient.GetAsync("api/topics");
        if (response.IsSuccessStatusCode)
        {
            var topics = await response.Content.ReadFromJsonAsync<List<TopicDto>>();
            return topics ?? new();
        }
        return new();
    }

    public async Task<List<PostDto>> GetPostsAsync(int topicId)
    {
        var response = await _httpClient.GetAsync($"api/posts?topicId={topicId}");
        if (response.IsSuccessStatusCode)
        {
            var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
            return posts ?? new();
        }
        return new();
    }

    private string? ExtractUsernameFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length >= 2)
            {
                var payloadJson = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=')) // исправляем Base64
                );

                using var doc = JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("unique_name", out var usernameProp))
                {
                    return usernameProp.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка парсинга токена: {ex.Message}");
        }
        return null;
    }
    public async Task<bool> CreateTopicAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrEmpty(_token))
            return false;

        try
        {
            var topic = new { Title = title };
            var response = await _httpClient.PostAsJsonAsync("api/topics", topic);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreatePostAsync(CreatePostDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content) || string.IsNullOrEmpty(_token))
            return false;

        try
        {
            
            var response = await _httpClient.PostAsJsonAsync("api/posts", dto);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ApprovePostAsync(int postId, bool approve)
    {
        if (string.IsNullOrEmpty(_token))
            return false;

        try
        {
            var response = await _httpClient.PatchAsync($"api/posts/{postId}/approve?approve={approve}", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}