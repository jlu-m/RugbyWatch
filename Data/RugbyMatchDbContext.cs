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
        public DbSet<Lineup> Lineups { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Club> Clubs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Match>()
                .HasOne(m => m.LocalTeam)
                .WithMany()
                .HasForeignKey(m => m.LocalTeamId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Match>()
                .HasOne(m => m.VisitorTeam)
                .WithMany()
                .HasForeignKey(m => m.VisitorTeamId)
                .OnDelete(DeleteBehavior.NoAction);


            base.OnModelCreating(modelBuilder);
        }
    }
}