using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class ScraperContext : DbContext
    {
        public ScraperContext(DbContextOptions<ScraperContext> options) : base(options)
        {
        }

        public DbSet<Show> Shows { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<ShowPerson> ShowPersons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShowPerson>()
                .HasKey(e => new {e.ShowId, e.PersonId});

            base.OnModelCreating(modelBuilder);
        }
    }
}