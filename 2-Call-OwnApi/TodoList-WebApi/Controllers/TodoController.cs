// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using TodoList_WebApi.Models;
using TodoList_WebApi.Options;
using TodoList_WebApi.Services;

namespace TodoList_WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    private ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadClaimsKey)]
    public IActionResult Get()
    {
        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
        {
            return BadRequest();
        }

        return Ok(_todoService.GetTodos(IsAppMakingRequest(), userIdentifier));
    }

    [HttpGet("{todoId}")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadClaimsKey)]
    public IActionResult Get(Guid todoId)
    {
        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
        {
            return BadRequest();
        }

        var todo = _todoService.GetTodo(IsAppMakingRequest(), todoId, userIdentifier);

        if (todo is null)
        {
            return NotFound();
        }

        return Ok(todo);
    }

    [HttpPost]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public IActionResult Post([FromBody] Todo todo)
    {
        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
        {
            return BadRequest();
        }

        var newTodoId = _todoService.AddTodo(
            IsAppMakingRequest(),
            todo,
            userIdentifier,
            HttpContext.User.GetDisplayName());

        if (newTodoId == Guid.Empty)
        {
            return Unauthorized();
        }

        return Ok(newTodoId);
    }

    [HttpPost("{todoId}")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public IActionResult Post(Guid todoId, [FromBody] Todo todo)
    {
        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
        {
            return BadRequest();
        }

        var matchingTodoId = _todoService.UpdateTodo(
            IsAppMakingRequest(),
            todoId,
            todo,
            userIdentifier,
            HttpContext.User.GetDisplayName());


        if (matchingTodoId == Guid.Empty)
        {
            return NotFound();
        }

        return Ok(matchingTodoId);
    }

    [HttpDelete("{todoId}")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public IActionResult Delete(Guid todoId)
    {
        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
        {
            return BadRequest();
        }

        if (!_todoService.DeleteTodo(IsAppMakingRequest(), todoId, userIdentifier))
        {
            return NotFound();
        }

        return Ok();
    }

    private bool IsAppMakingRequest()
    {
        // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
        //
        // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims
        return HttpContext.User
            .Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
    }
}
