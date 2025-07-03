using ESD_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace ESD_Project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<TrainLocation> TrainLocations { get; set; }
        public DbSet<PowerUsage> PowerUsages { get; set; }
        public DbSet<LoadWeight> LoadWeights { get; set; }
        public DbSet<DepotEnergySlot> DepotEnergySlots { get; set; }

        public DbSet<AlertDefinition> AlertDefinitions { get; set; }
       
        public DbSet<AlertHistory> AlertHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

         
        }
    }
}
