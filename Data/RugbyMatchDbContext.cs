using Microsoft.EntityFrameworkCore;

namespace RugbyWatch.Data
{
    public class RugbyMatchDbContext : DbContext
    {
        public RugbyMatchDbContext(DbContextOptions<RugbyMatchDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>()
                .HasKey(m => m.Id);


            base.OnModelCreating(modelBuilder);
        }
    }
}