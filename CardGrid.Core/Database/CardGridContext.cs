using CardGrid.Models;
using Microsoft.EntityFrameworkCore;

namespace CardGrid.Database
{
    /// <summary>
    /// EF Core context for the CardGrid database.
    /// </summary>
    /// <remarks>
    /// The mapping reproduces the schema EF6 created for the legacy app (<c>dbo.Employees</c>,
    /// <c>datetime</c> date columns, <c>nvarchar(max)</c> strings) so both hosts can share one database
    /// during the side-by-side migration. The context never creates or seeds the database implicitly;
    /// see <see cref="CardGridDatabaseInitializer"/>.
    /// </remarks>
    public class CardGridContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardGridContext"/> class.
        /// </summary>
        /// <param name="options">Provider and connection configuration.</param>
        public CardGridContext(DbContextOptions<CardGridContext> options)
            : base(options)
        {
        }

        /// <summary>Gets the employees table.</summary>
        public DbSet<Employee> Employees => Set<Employee>();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                // EF6 mapped DateTime to datetime; EF Core would default to datetime2.
                entity.Property(e => e.BirthDate).HasColumnType("datetime");
                entity.Property(e => e.HireDate).HasColumnType("datetime");

                entity.Ignore(e => e.PhotoUrl);
            });
        }
    }
}
