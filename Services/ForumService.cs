using System.Net.Http.Json;
using ForumClient.Models;

namespace ForumClient.Services;

public class ForumService
{
    private readonly HttpClient _httpClient;

    public ForumService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
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
        var request = new HttpRequestMessage(HttpMethod.Get, "api/topics");
        request.Headers.Authorization = new("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var topics = await response.Content.ReadFromJsonAsync<List<TopicDto>>();
            return topics ?? new();
        }
        return new();
    }

    public async Task<List<PostDto>> GetPostsAsync(int topicId, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/posts?topicId={topicId}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
            return posts ?? new();
        }
        return new();
    }
}