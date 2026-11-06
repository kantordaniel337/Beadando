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

    public async Task<List<CalendarTaskDto>> GetTasksAsync(DateTime from,DateTime to)
    {
        var fromValue = Uri.EscapeDataString(
            from.ToString("yyyy-MM-ddTHH:mm:ss"));

        var toValue = Uri.EscapeDataString(
            to.ToString("yyyy-MM-ddTHH:mm:ss"));

        var url = $"api/calendar?from={fromValue}&to={toValue}";

        var tasks = await _httpClient
            .GetFromJsonAsync<List<CalendarTaskDto>>(url);

        return tasks ?? [];
    }

    public async Task<RecurrenceDto> CreateRecurrenceAsync(int value,int unit)
    {
        var recurrence = new RecurrenceDto
        {
            Value = value,
            Unit = unit
        };

        var response = await _httpClient.PostAsJsonAsync("api/Recurrences", recurrence);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<RecurrenceDto>()
            ?? throw new Exception("Nem érkezett vissza az ismétlődés.");
    }
}