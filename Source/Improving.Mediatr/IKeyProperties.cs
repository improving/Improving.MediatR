namespace Improving.MediatR
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IKeyProperties<TId>
    {
        TId    Id   { get; set; }
        string Name { get; set; }
    }

    public static class KeyPropertyExtensions
    {
        public static string FindNameOrDefault<T>(this IEnumerable<IKeyProperties<T>> keyProperties, T id)
        {
            return keyProperties.FirstOrDefault(x => Equals(x.Id, id))?.Name ?? string.Empty;
        }

        public static string FindNameOrDefault<T>(this IEnumerable<Key<T>> keys, T id)
        {
            return keys.FirstOrDefault(k => Equals(k.Id, id))?.Name ?? string.Empty;
        }
    }
}
