using System.ComponentModel;
using BookPath.Application;
using BookPath.Domain;
using BookPath.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace BookPath.Commands;

public sealed class BookmarkCommand : Command<BookmarkCommand.Settings>
{
    private readonly BookmarkService _service;

    public BookmarkCommand()
        : this(new BookmarkService(new JsonBookmarkRepository()))
    {
    }

    public BookmarkCommand(BookmarkService service)
    {
        _service = service;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[bookmark]")]
        [Description("Bookmark name to navigate to. Omit it to create a bookmark.")]
        public string? Bookmark { get; init; }

        [CommandOption("-n|--name <NAME>")]
        [Description("Bookmark name when creating a bookmark.")]
        public string? Name { get; init; }

        [CommandOption("-p|--path <PATH>")]
        [Description("Path when creating a bookmark. Defaults to the current directory.")]
        public string? Path { get; init; }

        public override ValidationResult Validate()
        {
            if (Bookmark is not null && (Name is not null || Path is not null))
            {
                return ValidationResult.Error(
                    "A bookmark name cannot be combined with --name or --path. Use 'update' to change an existing bookmark.");
            }

            return ValidationResult.Success();
        }
    }

    protected override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation)
    {
        if (settings.Bookmark is not null)
        {
            Bookmark bookmark = _service.Get(settings.Bookmark);

            // The direct lookup intentionally emits only the path so a shell wrapper can cd to it.
            Console.WriteLine(bookmark.Path);
            return 0;
        }

        Bookmark created = _service.Create(settings.Name, settings.Path);

        AnsiConsole.MarkupLine(
            $"[green]Created[/] [bold]{CommandOutput.Escape(created.Name)}[/] -> [grey]{CommandOutput.Escape(created.Path)}[/]");

        return 0;
    }
}
