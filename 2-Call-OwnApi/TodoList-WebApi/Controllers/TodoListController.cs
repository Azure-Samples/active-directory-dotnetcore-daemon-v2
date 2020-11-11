// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Newtonsoft.Json;
using TodoList_WebApi.Models;

namespace TodoList_WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoListController : ControllerBase
    {
        // In-memory TodoList
        private static readonly Dictionary<int, TodoItem> TodoStore = new Dictionary<int, TodoItem>();

        public TodoListController()
        {
            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new TodoItem() { Id = 1, Task = "Pick up groceries" });
                TodoStore.Add(2, new TodoItem() { Id = 2, Task = "Finish invoice report" });
                TodoStore.Add(3, new TodoItem() { Id = 3, Task = "Water plants" });
            }
        }

        // GET: api/todolist
        [HttpGet]
        public IActionResult Get()
        {
            HttpContext.ValidateAppRole("DaemonAppRole");
            return Ok(TodoStore.Values);
        }
    }
}