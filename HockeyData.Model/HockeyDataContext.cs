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
			var connectionString = this.config["BasketballDataContextConnectionString"];
			optionsBuilder.UseSqlServer(connectionString);
		}

		public DbSet<League> Leagues { get; set; }
		public DbSet<Season> Seasons { get; set; }
		public DbSet<Team> Teams { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<League>(e =>
			{
				e.HasKey(x => x.LeagueId);
				e.Property(x => x.LeagueName).HasMaxLength(64);
				e.Property(x => x.LeagueAbbr).HasMaxLength(8);
			});

			modelBuilder.Entity<Season>(e =>
			{
				e.HasKey(x => x.SeasonId);
				e.HasOne(x => x.League).WithMany(y => y.Seasons).HasForeignKey(x => x.LeagueId);
				e.Property(x => x.NhlSeasonKey).HasMaxLength(8);
			});

			modelBuilder.Entity<Team>(e =>
			{
				e.HasKey(x => x.TeamId);
				e.Property(x => x.TeamFullName).HasMaxLength(64);
				e.Property(x => x.TeamLocation).HasMaxLength(32).IsRequired(false);
				e.Property(x => x.TeamName).HasMaxLength(32).IsRequired(false);
				e.Property(x => x.TeamShortName).HasMaxLength(32).IsRequired(false);
				e.Property(x => x.TeamAlias).HasMaxLength(4).IsRequired(false);
				e.Property(x => x.WebSiteUrl).HasMaxLength(255).IsRequired(false);
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
