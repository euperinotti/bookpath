namespace BookPath.Domain;

public sealed record Bookmark
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
}
