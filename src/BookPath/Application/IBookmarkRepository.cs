using BookPath.Domain;

namespace BookPath.Application;

public interface IBookmarkRepository
{
    IReadOnlyList<Bookmark> GetAll();
    void Save(IEnumerable<Bookmark> bookmarks);
}
