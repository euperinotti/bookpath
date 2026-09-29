using System.ComponentModel;
using BookPath.Application;
using BookPath.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace BookPath.Commands;

public sealed class UpdateCommand : Command<UpdateCommand.Settings>
{
    private readonly BookmarkService _service;

    public UpdateCommand()
        : this(new BookmarkService(new JsonBookmarkRepository()))
    {
    }

    public UpdateCommand(BookmarkService service)
    {
        _service = service;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<bookmark_name>")]
        [Description("Existing bookmark name.")]
        public string BookmarkName { get; init; } = string.Empty;

        [CommandOption("-n|--name <NAME>")]
        [Description("New bookmark name. Omit it to keep the current name.")]
        public string? Name { get; init; }

        [CommandOption("-p|--path <PATH>")]
        [Description("New bookmark path. Omit it to keep the current path.")]
        public string? Path { get; init; }

        public override ValidationResult Validate()
        {
            if (Name is null && Path is null)
                return ValidationResult.Error("Provide --name and/or --path.");

            return ValidationResult.Success();
        }
    }

    protected override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation)
    {
        var updated = _service.Update(settings.BookmarkName, settings.Name, settings.Path);

        AnsiConsole.MarkupLine(
            $"[green]Updated[/] [bold]{CommandOutput.Escape(updated.Name)}[/] -> [grey]{CommandOutput.Escape(updated.Path)}[/]");

        return 0;
    }
}
