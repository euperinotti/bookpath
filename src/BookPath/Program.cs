using BookPath;
using BookPath.Application;
using BookPath.Commands;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

ServiceCollection serviceCollection = new ServiceCollection();
TypeRegistrar registrar = new TypeRegistrar(serviceCollection);

var app = new CommandApp<BookmarkCommand>(registrar);

app.Configure(config =>
{
    config.SetApplicationName("bookpath");

    config.AddCommand<ListCommand>("list")
        .WithDescription("List all saved bookmarks.");
    config.AddCommand<UpdateCommand>("update")
        .WithDescription("Update an existing bookmark.");
    config.AddCommand<DeleteCommand>("delete")
        .WithDescription("Delete an existing bookmark.");
});

try
{
    return await app.RunAsync(args);
}
catch (BookmarkException ex)
{
    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
    return 1;
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Unexpected error:[/] {Markup.Escape(ex.Message)}");
    return 1;
}
