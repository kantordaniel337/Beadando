using System.Net.Http;
using System.Net.Http.Json;
using TasksWpf.Models;

namespace TasksWpf.Api;

public class CalendarApiClient
{
    private readonly HttpClient _httpClient;

    public CalendarApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5213/")
        };
    }

    public async Task<List<CalendarTaskDto>> GetTasksAsync()
    {
        var tasks = await _httpClient
            .GetFromJsonAsync<List<CalendarTaskDto>>("api/CalendarTasks");

        return tasks ?? [];
    }

    public async Task<CalendarTaskDto?> CreateTaskAsync(
        CalendarTaskDto task)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/CalendarTasks", task);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<CalendarTaskDto>();
    }

    public async Task UpdateTaskAsync(CalendarTaskDto task)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/CalendarTasks/{task.Id}", task);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTaskAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/CalendarTasks/{id}");

        response.EnsureSuccessStatusCode();
    }
}