namespace MockProjectService.Core.Extensions
{
    public interface IQueryExtensions<T> where T : class
    {
        IQueryable<T> ApplyIncludes(IQueryable<T> query, params string[] includePaths);
    }
}