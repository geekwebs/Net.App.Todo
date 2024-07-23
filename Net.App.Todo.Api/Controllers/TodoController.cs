using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.App.Todo.Models;

namespace Net.App.Todo.Api.Controllers;

[ApiController]
[Route("api/todos")]
[Authorize]
public class TodoController : ControllerBase
{
    private static readonly List<TodoX> Todos = new()
    {
        new TodoX {Id = 1, Title = "Sample Todo 1", IsCompleted = false},
        new TodoX {Id = 2, Title = "Sample Todo 2", IsCompleted = true},
        new TodoX {Id = 3, Title = "Sample Todo 3", IsCompleted = false},
    };
    
    [HttpGet]
    public IActionResult GetTodos()
    {
        return Ok(Todos);
    }

    [HttpGet("{id}")]
    public IActionResult GetTodoById(int id)
    {
        var todo = Todos.FirstOrDefault(x => x.Id == id);
        if (todo == null)
        {
            return NotFound();
        }
        return Ok(todo);
    }

    [HttpPost]
    public IActionResult CreateTodo([FromBody] TodoX todo)
    {
        todo.Id = Todos.Max(t => t.Id) + 1;
        Todos.Add(todo);
        return CreatedAtAction(nameof(GetTodoById), new {id = todo.Id}, todo);
    }

    [HttpPut]
    public IActionResult UpdateTodo(int id, [FromBody] TodoX updatedTodo)
    {
        var todo = Todos.FirstOrDefault(t => t.Id == id);
        if (todo == null)
        {
            return NotFound();
        }
        
        todo.Title = updatedTodo.Title;
        todo.IsCompleted = updatedTodo.IsCompleted;
        return NoContent();
    }
}
