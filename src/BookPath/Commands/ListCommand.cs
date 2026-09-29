using System.ComponentModel;
using BookPath.Application;
using BookPath.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace BookPath.Commands;

public sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly BookmarkService _service;

    public ListCommand()
        : this(new BookmarkService(new JsonBookmarkRepository()))
    {
    }

    public ListCommand(BookmarkService service)
    {
        _service = service;
    }

    public sealed class Settings : CommandSettings
    {
    }

    protected override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation)
    {
        var bookmarks = _service.List();

        if (bookmarks.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No bookmarks found.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Name")
            .AddColumn("Path");

        foreach (var bookmark in bookmarks)
        {
            table.AddRow(
                CommandOutput.Escape(bookmark.Name),
                CommandOutput.Escape(bookmark.Path));
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
