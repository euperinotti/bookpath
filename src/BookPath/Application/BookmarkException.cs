namespace BookPath.Application;

public sealed class BookmarkException : Exception
{
    public BookmarkException(string message) : base(message)
    {
    }
}
