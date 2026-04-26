namespace TaskManagement.DAL.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> PageBy<T>(this IQueryable<T> resultSet, int pageNumber, int pageSize)
        {
            return resultSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
