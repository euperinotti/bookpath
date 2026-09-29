using BookPath.Domain;

namespace BookPath.Application;

public sealed class BookmarkService
{
    private readonly IBookmarkRepository _repository;

    public BookmarkService(IBookmarkRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Bookmark> List()
        => _repository.GetAll()
            .OrderBy(GetDisplayOrder)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public Bookmark Create(string? name, string? path)
    {
        var bookmarks = _repository.GetAll().ToList();
        var finalName = string.IsNullOrWhiteSpace(name)
            ? GetNextNumericName(bookmarks)
            : name.Trim();
        var finalPath = NormalizePath(path);

        EnsureNameIsAvailable(bookmarks, finalName);
        EnsurePathIsAvailable(bookmarks, finalPath);

        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid(),
            Name = finalName,
            Path = finalPath,
        };

        bookmarks.Add(bookmark);
        _repository.Save(bookmarks);
        return bookmark;
    }

    public Bookmark Update(string bookmarkName, string? newName, string? newPath)
    {
        var bookmarks = _repository.GetAll().ToList();
        var bookmark = Find(bookmarks, bookmarkName);

        if (newName is not null)
        {
            var normalizedName = newName.Trim();
            if (normalizedName.Length == 0)
                throw new BookmarkException("The new bookmark name cannot be empty.");

            if (!string.Equals(bookmark.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
                EnsureNameIsAvailable(bookmarks, normalizedName, bookmark.Id);

            bookmark = bookmark with { Name = normalizedName };
        }

        if (newPath is not null)
        {
            var normalizedPath = NormalizePath(newPath);

            if (!PathsEqual(bookmark.Path, normalizedPath))
                EnsurePathIsAvailable(bookmarks, normalizedPath, bookmark.Id);

            bookmark = bookmark with { Path = normalizedPath };
        }

        if (newName is null && newPath is null)
            throw new BookmarkException("Provide --name and/or --path to update the bookmark.");

        var index = bookmarks.FindIndex(x => x.Id == bookmark.Id);
        bookmarks[index] = bookmark;
        _repository.Save(bookmarks);

        return bookmark;
    }

    public Bookmark Delete(string bookmarkName)
    {
        var bookmarks = _repository.GetAll().ToList();
        var bookmark = Find(bookmarks, bookmarkName);
        bookmarks.RemoveAll(x => x.Id == bookmark.Id);
        _repository.Save(bookmarks);
        return bookmark;
    }

    public Bookmark Get(string bookmarkName)
    {
        var bookmark = Find(_repository.GetAll(), bookmarkName);

        if (!Directory.Exists(bookmark.Path))
            throw new BookmarkException($"The bookmark path does not exist: {bookmark.Path}");

        return bookmark;
    }

    private static Bookmark Find(IEnumerable<Bookmark> bookmarks, string bookmarkName)
    {
        var bookmark = bookmarks.FirstOrDefault(x =>
            string.Equals(x.Name, bookmarkName.Trim(), StringComparison.OrdinalIgnoreCase));

        return bookmark
            ?? throw new BookmarkException($"Bookmark '{bookmarkName}' was not found.");
    }

    private static void EnsureNameIsAvailable(
        IEnumerable<Bookmark> bookmarks,
        string name,
        Guid? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BookmarkException("The bookmark name cannot be empty.");

        if (bookmarks.Any(x =>
                x.Id != currentId &&
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BookmarkException($"A bookmark named '{name}' already exists.");
        }
    }

    private static void EnsurePathIsAvailable(
        IEnumerable<Bookmark> bookmarks,
        string path,
        Guid? currentId = null)
    {
        if (bookmarks.Any(x =>
                x.Id != currentId &&
                PathsEqual(x.Path, path)))
        {
            throw new BookmarkException($"The path '{path}' is already bookmarked.");
        }
    }

    private static string GetNextNumericName(IEnumerable<Bookmark> bookmarks)
    {
        var max = bookmarks
            .Select(x => int.TryParse(x.Name, out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();

        return checked(max + 1).ToString();
    }

    private static int GetDisplayOrder(Bookmark bookmark)
        => int.TryParse(bookmark.Name, out var number) ? number : int.MaxValue;

    private static string NormalizePath(string? path)
    {
        var value = string.IsNullOrWhiteSpace(path)
            ? Directory.GetCurrentDirectory()
            : path.Trim();

        try
        {
            return TrimTrailingDirectorySeparators(Path.GetFullPath(value));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new BookmarkException($"Invalid path '{value}'.");
        }
    }

    private static string TrimTrailingDirectorySeparators(string path)
    {
        var root = Path.GetPathRoot(path);
        if (!string.IsNullOrEmpty(root) && string.Equals(path, root, StringComparison.OrdinalIgnoreCase))
            return path;

        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool PathsEqual(string left, string right)
        => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
