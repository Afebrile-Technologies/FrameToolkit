namespace FrameToolkit.Extensions;

public static class EnumerableExtension
{
    public static IQueryable<TSource> SearchFilter<TSource>(this IQueryable<TSource> source,
        Dictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
            return source;

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TSource), "x");
        System.Linq.Expressions.Expression? predicate = null;

        foreach (var filter in filters)
        {
            var property = System.Linq.Expressions.Expression.PropertyOrField(parameter, filter.Key);
            var value = System.Linq.Expressions.Expression.Constant(filter.Value);

            // Convert property to string for comparison
            var propertyAsString = System.Linq.Expressions.Expression.Call(
                property,
                typeof(object).GetMethod("ToString")!
            );

            var condition = System.Linq.Expressions.Expression.Equal(propertyAsString, value);

            predicate = predicate == null
                ? condition
                : System.Linq.Expressions.Expression.AndAlso(predicate, condition);
        }

        var lambda = System.Linq.Expressions.Expression.Lambda<Func<TSource, bool>>(predicate!, parameter);
        return source.Where(lambda);
    }
}
