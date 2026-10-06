using System.ComponentModel;
using System.Diagnostics;
using BookPath.Application;
using BookPath.Domain;
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
        long fetchedResultsMs = 0;
        long renderResultsMs = 0;

        Stopwatch sw = Stopwatch.StartNew();

        IReadOnlyList<Bookmark> bookmarks = _service.List();

        if (bookmarks.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No bookmarks found.[/]");
            return 0;
        }

        fetchedResultsMs = sw.ElapsedMilliseconds;

        sw.Restart();

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Name")
            .AddColumn("Path");

        foreach (Bookmark bookmark in bookmarks)
        {
            table.AddRow(
                CommandOutput.Escape(bookmark.Name),
                CommandOutput.Escape(bookmark.Path));
        }

        AnsiConsole.Write(table);

        renderResultsMs = sw.ElapsedMilliseconds;

        AnsiConsole.WriteLine($"Fetched results in {fetchedResultsMs} ms");
        AnsiConsole.WriteLine($"Rendered results in {renderResultsMs} ms");
        return 0;
    }
}
