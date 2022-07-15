// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace daemon_console.Models;

public class Todo
{
    [JsonPropertyNameAttribute("id")]
    public Guid Id { get; set; }

    [JsonPropertyNameAttribute("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyNameAttribute("title")]
    public string Title { get; set; }

    [JsonPropertyNameAttribute("owner")]
    public string Owner { get; set; }
}