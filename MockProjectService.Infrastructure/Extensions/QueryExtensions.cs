using Microsoft.EntityFrameworkCore;
using MockProjectService.Core.Extensions;

namespace MockProjectService.Infrastructure.Extensions
{
    public class QueryExtensions<T> : IQueryExtensions<T> where T : class
    {
        public IQueryable<T> ApplyIncludes(IQueryable<T> query, params string[] includePaths)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (includePaths == null || includePaths.Length == 0) return query;

            foreach (var path in includePaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    query = query.Include(path.Trim());
                }
            }

            return query;
        }
    }
}