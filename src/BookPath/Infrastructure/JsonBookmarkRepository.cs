using System.Text.Json;
using BookPath.Application;
using BookPath.Domain;

namespace BookPath.Infrastructure;

public sealed class JsonBookmarkRepository : IBookmarkRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;

    public JsonBookmarkRepository(string? filePath = null)
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(home))
            throw new InvalidOperationException("Could not determine the current user's home directory.");

        _filePath = filePath
            ?? Path.Combine(home, ".bookpath", "bookmarks.json");
    }

    public string FilePath => _filePath;

    public IReadOnlyList<Bookmark> GetAll()
    {
        if (!File.Exists(_filePath))
            return Array.Empty<Bookmark>();

        try
        {
            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<Bookmark>();

            return JsonSerializer.Deserialize<List<Bookmark>>(json, JsonOptions)
                ?? new List<Bookmark>();
        }
        catch (JsonException ex)
        {
            throw new BookmarkException(
                $"The bookmark file contains invalid JSON: {_filePath}{Environment.NewLine}{ex.Message}");
        }
        catch (IOException ex)
        {
            throw new BookmarkException(
                $"Could not read the bookmark file '{_filePath}': {ex.Message}");
        }
    }

    public void Save(IEnumerable<Bookmark> bookmarks)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new BookmarkException("Could not determine the bookmark directory.");

        Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(bookmarks.OrderBy(x => x.Name), JsonOptions);
        string temporaryFile = $"{_filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllText(temporaryFile, json);
            File.Move(temporaryFile, _filePath, overwrite: true);
        }
        catch (IOException ex)
        {
            throw new BookmarkException(
                $"Could not save the bookmark file '{_filePath}': {ex.Message}");
        }
        finally
        {
            if (File.Exists(temporaryFile))
            {
                try
                {
                    File.Delete(temporaryFile);
                }
                catch
                {
                    // Do not hide the original save error.
                }
            }
        }
    }
}
