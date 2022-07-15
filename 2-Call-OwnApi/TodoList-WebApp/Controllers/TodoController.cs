using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using TodoList_WebApp.Services;
using TodoList_WebApp.Models;

namespace TodoList_WebApp.Controllers;

public class TodoController : Controller
{
    private ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<ActionResult> Index()
    {
        return View(await _todoService.GetAsync());
    }

    [HttpGet]
    public async Task<ActionResult> Details(Guid id)
    {
        return View(await _todoService.GetAsync(id));
    }

    [HttpGet]
    public ActionResult Create()
    {
        Todo todo = new Todo() { Owner = HttpContext.User.GetDisplayName() };
        return View(todo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind("Title,Owner")] Todo todo)
    {
        todo.UserId = Guid.Parse(HttpContext.User.GetHomeObjectId()!);
        todo.Owner = HttpContext.User.GetDisplayName();
        await _todoService.AddAsync(todo);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<ActionResult> Edit(Guid id)
    {
        Todo todo = await this._todoService.GetAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        return View(todo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Guid id, [Bind("Id,Title")] Todo todo)
    {
        todo.UserId = Guid.Parse(HttpContext.User.GetHomeObjectId()!);
        todo.Owner = HttpContext.User.GetDisplayName();
        await _todoService.EditAsync(todo);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<ActionResult> Delete(Guid id)
    {
        Todo todo = await _todoService.GetAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        return View(todo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(Guid id, [Bind("Id,Title,Owner")] Todo todo)
    {
        await this._todoService.DeleteAsync(id);

        return RedirectToAction("Index");
    }
}