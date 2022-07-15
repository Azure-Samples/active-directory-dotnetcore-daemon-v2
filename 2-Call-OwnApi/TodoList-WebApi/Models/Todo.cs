// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace TodoList_WebApi.Models;

public class Todo
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Owner { get; set; }
}
