using System.Linq.Expressions;
using CardGrid.Database;
using CardGrid.Models;

namespace CardGrid.Core.Services
{
    /// <summary>
    /// Builds filtered, sorted, paged employee results for the card grid.
    /// </summary>
    public sealed class EmployeeGridService
    {
        /// <summary>Upper bound on page size, guarding against unbounded result sets.</summary>
        public const int MaxPageSize = 100;

        /// <summary>Employee source.</summary>
        private readonly IEmployeeStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeGridService"/> class.
        /// </summary>
        /// <param name="store">Employee source.</param>
        public EmployeeGridService(IEmployeeStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Returns one page of employees matching <paramref name="request"/>.
        /// </summary>
        /// <remarks>
        /// Differences from the legacy action, all deliberate:
        /// an empty search returns every row (legacy dropped rows with null text columns);
        /// <c>page</c> is clamped to at least 1 and <c>rows</c> to 1..<see cref="MaxPageSize"/>;
        /// ties are broken by <c>Id</c> so pages are stable.
        /// </remarks>
        /// <param name="request">Search, sort and paging parameters.</param>
        /// <returns>The requested page plus paging metadata.</returns>
        public GridResponse<Employee> GetPage(GridRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var page = Math.Max(1, request.Page);
            var rows = Math.Clamp(request.Rows, 1, MaxPageSize);
            var sidx = string.IsNullOrWhiteSpace(request.Sidx) ? "Id" : request.Sidx.Trim();
            var ascending = !string.Equals(request.Sord?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            var search = request.SearchParam?.Trim() ?? string.Empty;

            var query = Filter(this.store.Employees, search);
            var total = query.Count();

            var skip = (long)(page - 1) * rows;
            var data = skip >= total
                ? new List<Employee>()
                : Sort(query, sidx, ascending).Skip((int)skip).Take(rows).ToList();

            return new GridResponse<Employee>
            {
                rows = rows,
                searchParam = search,
                sidx = sidx,
                page = page,
                total = total,
                sord = ascending ? SortDirection.asc : SortDirection.desc,
                Data = data,
            };
        }

        /// <summary>
        /// Restricts <paramref name="source"/> to employees whose id equals, or whose text fields
        /// contain, <paramref name="search"/>.
        /// </summary>
        /// <param name="source">Unfiltered employees.</param>
        /// <param name="search">Trimmed search term; empty means no filter.</param>
        /// <returns>The filtered query.</returns>
        private static IQueryable<Employee> Filter(IQueryable<Employee> source, string search)
        {
            if (search.Length == 0)
            {
                return source;
            }

            return source.Where(e =>
                e.Id.ToString() == search
                || e.Email.Contains(search)
                || e.ContactNo.Contains(search)
                || e.BirthDate.ToString().Contains(search)
                || e.JobTitle.Contains(search)
                || e.Name.Contains(search));
        }

        /// <summary>
        /// Orders <paramref name="source"/> by the column named <paramref name="sidx"/>, then by <c>Id</c>.
        /// Only whitelisted columns are sortable; anything else sorts by <c>Id</c>.
        /// </summary>
        /// <param name="source">Query to order.</param>
        /// <param name="sidx">Column name, case-insensitive.</param>
        /// <param name="ascending">Sort direction for the primary key column.</param>
        /// <returns>The ordered query.</returns>
        private static IOrderedQueryable<Employee> Sort(IQueryable<Employee> source, string sidx, bool ascending)
        {
            return sidx.ToLowerInvariant() switch
            {
                "email" => OrderBy(source, e => e.Email, ascending),
                "contactno" => OrderBy(source, e => e.ContactNo, ascending),
                "birthdate" => OrderBy(source, e => e.BirthDate, ascending),
                "jobtitle" => OrderBy(source, e => e.JobTitle, ascending),
                "name" => OrderBy(source, e => e.Name, ascending),
                "gender" => OrderBy(source, e => e.Gender, ascending),
                "hiredate" => OrderBy(source, e => e.HireDate, ascending),
                _ => ascending ? source.OrderBy(e => e.Id) : source.OrderByDescending(e => e.Id),
            };
        }

        /// <summary>Orders by <paramref name="key"/> in the given direction, with <c>Id</c> as tie-breaker.</summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="source">Query to order.</param>
        /// <param name="key">Primary sort key.</param>
        /// <param name="ascending">Direction for <paramref name="key"/>.</param>
        /// <returns>The ordered query.</returns>
        private static IOrderedQueryable<Employee> OrderBy<TKey>(
            IQueryable<Employee> source,
            Expression<Func<Employee, TKey>> key,
            bool ascending)
        {
            var ordered = ascending ? source.OrderBy(key) : source.OrderByDescending(key);
            return ordered.ThenBy(e => e.Id);
        }
    }
}
