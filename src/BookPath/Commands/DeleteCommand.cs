using System.ComponentModel;
using BookPath.Application;
using BookPath.Domain;
using BookPath.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace BookPath.Commands;

public sealed class DeleteCommand : Command<DeleteCommand.Settings>
{
    private readonly BookmarkService _service;

    public DeleteCommand()
        : this(new BookmarkService(new JsonBookmarkRepository()))
    {
    }

    public DeleteCommand(BookmarkService service)
    {
        _service = service;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<bookmark_name>")]
        [Description("Bookmark name to delete.")]
        public string BookmarkName { get; init; } = string.Empty;
    }

    protected override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation)
    {
        Bookmark deleted = _service.Delete(settings.BookmarkName);

        AnsiConsole.MarkupLine(
            $"[green]Deleted[/] bookmark [bold]{CommandOutput.Escape(deleted.Name)}[/].");

        return 0;
    }
}
