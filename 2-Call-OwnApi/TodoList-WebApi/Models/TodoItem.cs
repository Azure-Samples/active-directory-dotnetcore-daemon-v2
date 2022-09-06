// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TodoList_WebApi.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Task { get; set; }
        public string Owner { get; set; }
    }
}
