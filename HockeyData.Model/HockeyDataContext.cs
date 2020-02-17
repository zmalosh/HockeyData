using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using Microsoft.EntityFrameworkCore;

namespace HockeyData.Model
{
	public class HockeyDataContext : DbContext
	{
		private readonly IConfiguration config;

		public HockeyDataContext(IConfiguration config) : base()
		{
			this.config = config;
			this.ChangeTracker.Tracked += OnEntityTracked;
			this.ChangeTracker.StateChanged += OnEntityStateChanged;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//IConfigurationRoot configuration = new ConfigurationBuilder()
			//	.SetBasePath(Directory.GetCurrentDirectory())
			//	.AddJsonFile("appsettings.json")
			//	.Build();
			var connectionString = this.config["HockeyDataContextConnectionString"];
			optionsBuilder.UseSqlServer(connectionString);
		}

		public DbSet<RefGameType> RefGameTypes { get; set; }
		public DbSet<RefGameStatus> RefGameStatuses { get; set; }
		public DbSet<League> Leagues { get; set; }
		public DbSet<Season> Seasons { get; set; }
		public DbSet<Team> Teams { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<RefGameType>(e =>
			{
				e.HasKey(x => x.GameTypeId);
				e.Property(x => x.GameTypeId).ValueGeneratedNever();
				e.Property(x => x.NhlGameTypeKey).HasMaxLength(16);
				e.Property(x => x.GameTypeDescription).HasMaxLength(128);
			});

			modelBuilder.Entity<RefGameStatus>(e =>
			{
				e.HasKey(x => x.DetailedGameStatusId);
				e.Property(x => x.DetailedGameStatusId).ValueGeneratedNever();
				e.Property(x => x.GameStatusName).HasMaxLength(16);
				e.Property(x => x.DetailedGameStatusName).HasMaxLength(64);
			});

			modelBuilder.Entity<League>(e =>
			{
				e.HasKey(x => x.LeagueId);
				e.Property(x => x.LeagueName).HasMaxLength(64);
				e.Property(x => x.LeagueAbbr).HasMaxLength(8);
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<Season>(e =>
			{
				e.HasKey(x => x.SeasonId);
				e.HasOne(x => x.League).WithMany(y => y.Seasons).HasForeignKey(x => x.LeagueId);
				e.Property(x => x.NhlSeasonKey).HasMaxLength(8);
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<Team>(e =>
			{
				e.HasKey(x => x.TeamId);
				e.Property(x => x.TeamFullName).HasMaxLength(64);
				e.Property(x => x.TeamLocation).HasMaxLength(32).IsRequired(false);
				e.Property(x => x.TeamName).HasMaxLength(32).IsRequired(false);
				e.Property(x => x.TeamShortName).HasMaxLength(32).IsRequired(false);
				e.Property(x => x.TeamAbbr).HasMaxLength(4).IsRequired(false);
				e.Property(x => x.WebSiteUrl).HasMaxLength(255).IsRequired(false);
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});
		}

		void OnEntityTracked(object sender, EntityTrackedEventArgs e)
		{
			if (!e.FromQuery && e.Entry.State == EntityState.Added && e.Entry.Entity is IEntity entity)
			{
				entity.DateCreatedUtc = DateTime.UtcNow;
				entity.DateLastModifiedUtc = DateTime.UtcNow;
			}
		}

		void OnEntityStateChanged(object sender, EntityStateChangedEventArgs e)
		{
			if (e.NewState == EntityState.Modified && e.Entry.Entity is IEntity entity)
			{
				entity.DateLastModifiedUtc = DateTime.UtcNow;
			}
		}
	}
}
