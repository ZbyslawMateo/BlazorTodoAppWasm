using System.Reflection.Metadata;
using Application.DaoInterfaces;
using Shared.DTOs;
using Shared.Models;

namespace FileData.DAOs;

public class TodoFileDao : ITodoDao
{
    private readonly FileContext context;

    public TodoFileDao(FileContext context)
    {
        this.context = context;
    }
    public Task<Todo> CreateAsync(Todo todo)
    {
        int id = 1;
        if (context.Todos.Any())
        {
            id = context.Todos.Max(t => t.Id);
            id++;
        }

        todo.Id = id;
        
        context.Todos.Add(todo);
        context.SaveChanges();

        return Task.FromResult(todo);
    }

    public Task<IEnumerable<Todo>> GetAsync(SearchTodoParametersDto searchParameters)
    {
        IEnumerable<Todo> todos = context.Todos.AsEnumerable();

        if (!string.IsNullOrEmpty(searchParameters.Username))
        {
            // we know that username is unique, so just fetch the first
            todos = context.Todos.Where(todo =>
                todo.Owner.UserName.Equals(searchParameters.Username, StringComparison.OrdinalIgnoreCase));
        }

        if (searchParameters.UserId != null)
        {
            todos = todos.Where(t => t.Owner.Id == searchParameters.UserId);
        }

        if (searchParameters.CompletedStatus == null)
        {
            todos = todos.Where(t => t.IsCompleted == searchParameters.CompletedStatus);
        }

        if (!string.IsNullOrEmpty(searchParameters.TitleContains))
        {
            todos = todos.Where(t =>
                t.Title.Contains(searchParameters.TitleContains, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(todos);
    }

    public Task<Todo?> GetByIdAsync(int todoId)
    {
        Todo? existing = context.Todos.FirstOrDefault(t => t.Id == todoId);
        return Task.FromResult(existing);
    }

    public Task UpdateAsync(Todo toUpdate)
    {
        Todo? existing = context.Todos.FirstOrDefault(todo => todo.Id == toUpdate.Id);

        if (existing == null)
        {
            throw new Exception($"Todo with if {toUpdate.Id} does not exist.");
        }

        context.Todos.Remove(existing);
        context.Todos.Add(toUpdate);
        
        context.SaveChanges();

        return Task.CompletedTask;
    }
}