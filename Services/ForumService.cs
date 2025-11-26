using System.Net.Http.Json;
using ForumClient.Models;

namespace ForumClient.Services;

public class ForumService
{
    private readonly HttpClient _http;

    public ForumService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Token;
            }
        }
        catch { }
        return null;
    }

    public async Task<List<TopicDto>> GetTopicsAsync(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return await _http.GetFromJsonAsync<List<TopicDto>>("api/topics") ?? new();
    }

    public async Task<List<PostDto>> GetPostsAsync(int topicId, string token)
    {
        _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return await _http.GetFromJsonAsync<List<PostDto>>($"api/posts?topicId={topicId}") ?? new();
    }
}