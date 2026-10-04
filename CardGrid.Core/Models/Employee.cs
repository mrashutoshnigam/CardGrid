namespace CardGrid.Models
{
    /// <summary>
    /// Employee record displayed by the card grid. Mapped by EF Core to the <c>dbo.Employees</c> table.
    /// </summary>
    /// <remarks>
    /// The namespace intentionally matches the legacy <c>CardGrid</c> project so the EF Core context
    /// and the legacy EF6 context map the same table while both hosts share one database.
    /// </remarks>
    public class Employee
    {
        /// <summary>Gets or sets the primary key.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the employee's display name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the employee's job title.</summary>
        public string JobTitle { get; set; }

        /// <summary>Gets or sets the date of birth, when known.</summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>Gets or sets the date the employee was hired.</summary>
        public DateTime HireDate { get; set; }

        /// <summary>Gets or sets the employee's gender.</summary>
        public string Gender { get; set; }

        /// <summary>Gets or sets the employee's contact phone number.</summary>
        public string ContactNo { get; set; }

        /// <summary>Gets or sets the employee's email address.</summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets the relative URL of the employee's photo. Computed, not persisted
        /// (explicitly ignored in <c>CardGridContext</c>).
        /// </summary>
        public string PhotoUrl => @"\Photos\" + Id + ".jpg";
    }
}
