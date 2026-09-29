using Spectre.Console;

namespace BookPath.Commands;

internal static class CommandOutput
{
    public static string Escape(string value)
        => Markup.Escape(value);
}
