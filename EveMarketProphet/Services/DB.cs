using EveMarketProphet.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace EveMarketProphet.Services
{
    public class SdeContext : DbContext
    {
        public DbSet<Type> Types { get; set; }
        public DbSet<ContrabandType> ContrabandTypes { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<SolarSystem> SolarSystems { get; set; }
        public DbSet<SolarSystemJump> SolarSystemJumps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Data/sqlite-latest.sqlite");
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolarSystemJump>().HasKey(t => new { t.fromSolarSystemID, t.toSolarSystemID });
            modelBuilder.Entity<ContrabandType>().HasKey(t => new {t.factionID, t.typeID});
        }
    }

    public class Db
    {
        public static Db Instance { get; } = new Db();

        public List<Models.Type> Types { get; private set; }
        public List<ContrabandType> ContrabandTypes { get; private set; }
        public List<Station> Stations { get; private set; }
        public List<Region> Regions { get; private set; }
        public List<SolarSystem> SolarSystems { get; private set; }
        public List<SolarSystemJump> SolarSystemJumps { get; private set; }

        private Db()
        {
            using (var context = new SdeContext())
            {
                Types = context.Types.ToList();
                ContrabandTypes = context.ContrabandTypes.ToList();
                Stations = context.Stations.ToList();
                Regions = context.Regions.ToList();
                SolarSystems = context.SolarSystems.ToList();
                SolarSystemJumps = context.SolarSystemJumps.ToList();
            }
        }
    }

}
