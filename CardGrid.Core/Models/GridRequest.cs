namespace CardGrid.Models
{
    /// <summary>
    /// Query-string parameters sent by the card grid client when requesting a page of data.
    /// Defaults mirror the legacy <c>DefaultController.GetData</c> signature.
    /// </summary>
    public class GridRequest
    {
        /// <summary>Gets or sets the free-text search term; empty or whitespace means no filter.</summary>
        public string SearchParam { get; set; }

        /// <summary>Gets or sets the sort field name (case-insensitive). Unknown values fall back to <c>Id</c>.</summary>
        public string Sidx { get; set; } = "Id";

        /// <summary>Gets or sets the sort direction, <c>asc</c> or <c>desc</c>.</summary>
        public string Sord { get; set; } = "asc";

        /// <summary>Gets or sets the 1-based page index.</summary>
        public int Page { get; set; } = 1;

        /// <summary>Gets or sets the requested page size.</summary>
        public int Rows { get; set; } = 10;
    }
}
