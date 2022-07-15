using System.Text.Json.Serialization;

namespace TodoList_WebApp.Models;

public class Todo
{
    [JsonPropertyNameAttribute("id")]
    public Guid Id { get; set; }

    [JsonPropertyNameAttribute("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyNameAttribute("title")]
    public string? Title { get; set; }

    [JsonPropertyNameAttribute("owner")]
    public string? Owner { get; set; }
}