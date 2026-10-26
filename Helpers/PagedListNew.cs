using Microsoft.EntityFrameworkCore;
namespace ShopMvc.Helpers;
public class PagedList<T> : List<T>
{
    public int PageIndex { get; }
    public int TotalPages { get; }
    public PagedList(IEnumerable<T> items, int count, int page, int size) : base(items) { PageIndex = page; TotalPages = (int)Math.Ceiling(count / (double)size); }
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int size) { page = Math.Max(page, 1); var count = await query.CountAsync(); return new PagedList<T>(await query.Skip((page - 1) * size).Take(size).ToListAsync(), count, page, size); }
}
