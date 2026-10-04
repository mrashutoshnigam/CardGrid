namespace CardGrid.Models
{
    /// <summary>
    /// Sort direction understood by the card grid client. Serialized as its numeric value
    /// (<c>0</c>/<c>1</c>), matching the legacy <c>JavaScriptSerializer</c> output.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>Ascending order.</summary>
        asc,

        /// <summary>Descending order.</summary>
        desc
    }

    /// <summary>
    /// One page of grid data plus the paging/sorting state the client needs to render pagination.
    /// </summary>
    /// <remarks>
    /// Property names are lower-case on purpose: the JSON contract consumed by <c>CardGrid.js</c>
    /// predates this port and is serialized without a naming policy.
    /// </remarks>
    /// <typeparam name="T">Row type.</typeparam>
    public class GridResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridResponse{T}"/> class with the legacy defaults.
        /// </summary>
        public GridResponse()
        {
            total = 12;
            rows = 1;
            page = 1;
            sord = SortDirection.asc;
            sidx = searchParam = string.Empty;
            Data = new List<T>();
        }

        /// <summary>Gets the total number of pages; <c>0</c> when <see cref="rows"/> is not positive.</summary>
        public int noOfPages => rows > 0 ? (int)Math.Ceiling((double)total / rows) : 0;

        /// <summary>Gets or sets the total number of records matching the search.</summary>
        public int total { get; set; }

        /// <summary>Gets or sets the page size.</summary>
        public int rows { get; set; }

        /// <summary>Gets or sets the 1-based current page index.</summary>
        public int page { get; set; }

        /// <summary>Gets or sets the sort direction.</summary>
        public SortDirection sord { get; set; }

        /// <summary>Gets or sets the sort field.</summary>
        public string sidx { get; set; }

        /// <summary>Gets or sets the rows for the current page.</summary>
        public List<T> Data { get; set; }

        /// <summary>Gets or sets the search text the page was filtered with.</summary>
        public string searchParam { get; set; }
    }
}
