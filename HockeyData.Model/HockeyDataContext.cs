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
		public DbSet<Player> Players { get; set; }
		public DbSet<Game> Games { get; set; }
		public DbSet<SkaterBoxscore> SkaterBoxscores { get; set; }
		public DbSet<GoalieBoxscore> GoalieBoxscores { get; set; }
		public DbSet<TeamBoxscore> TeamBoxscores { get; set; }

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

			modelBuilder.Entity<Player>(e =>
			{
				e.HasKey(x => x.PlayerId);
				e.Property(x => x.BirthCity).HasMaxLength(128);
				e.Property(x => x.BirthCountry).HasMaxLength(3);
				e.Property(x => x.BirthState).HasMaxLength(2).IsRequired(false);
				e.Property(x => x.FirstName).HasMaxLength(64);
				e.Property(x => x.FullName).HasMaxLength(128);
				e.Property(x => x.LastName).HasMaxLength(64);
				e.Property(x => x.Nationality).HasMaxLength(3);
				e.Property(x => x.PrimaryPosition).HasMaxLength(3).IsRequired(false);
				e.Property(x => x.BirthDate).HasColumnType("date");
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<Game>(e =>
			{
				e.HasKey(x => x.GameId);
				e.HasOne(x => x.Season).WithMany(y => y.Games).HasForeignKey(x => x.SeasonId);
				e.HasOne(x => x.HomeTeam).WithMany(y => y.HomeGames).HasForeignKey(x => x.HomeTeamId).IsRequired(false);
				e.HasOne(x => x.AwayTeam).WithMany(y => y.AwayGames).HasForeignKey(x => x.AwayTeamId).IsRequired(false);
				e.Property(x => x.HomeCoachName).HasMaxLength(128).IsRequired(false).HasDefaultValue(null);
				e.Property(x => x.AwayCoachName).HasMaxLength(128).IsRequired(false).HasDefaultValue(null);
				e.Property(x => x.RefereeName1).HasMaxLength(128).IsRequired(false).HasDefaultValue(null);
				e.Property(x => x.RefereeName2).HasMaxLength(128).IsRequired(false).HasDefaultValue(null);
				e.Property(x => x.LinesmanName1).HasMaxLength(128).IsRequired(false).HasDefaultValue(null);
				e.Property(x => x.LinesmanName2).HasMaxLength(128).IsRequired(false).HasDefaultValue(null);
				e.Property(x => x.GameDateEst).HasColumnType("date");
				e.Property(x => x.GameTimeUtc).HasColumnType("datetime");
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<SkaterBoxscore>(e =>
			{
				e.HasKey(x => new { x.GameId, x.PlayerId });
				e.HasOne(x => x.Game).WithMany(y => y.PlayerBoxscores).HasForeignKey(x => x.GameId);
				e.HasOne(x => x.Player).WithMany(y => y.PlayerBoxscores).HasForeignKey(x => x.PlayerId);
				e.HasOne(x => x.Team).WithMany(y => y.PlayerBoxscores).HasForeignKey(x => x.TeamId);
				e.Property(x => x.Position).HasMaxLength(3).IsRequired(false);
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<GoalieBoxscore>(e =>
			{
				e.HasKey(x => new { x.GameId, x.PlayerId });
				e.HasOne(x => x.Game).WithMany(y => y.GoalieBoxscores).HasForeignKey(x => x.GameId);
				e.HasOne(x => x.Player).WithMany(y => y.GoalieBoxscores).HasForeignKey(x => x.PlayerId);
				e.HasOne(x => x.Team).WithMany(y => y.GoalieBoxscores).HasForeignKey(x => x.TeamId);
				e.Property(x => x.Decision).HasMaxLength(2).IsRequired(false);
				e.Property(x => x.DateCreatedUtc).HasColumnType("datetime");
				e.Property(x => x.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<TeamBoxscore>(e =>
			{
				e.HasKey(x => new { x.GameId, x.TeamId });
				e.HasOne(x => x.Team).WithMany(y => y.TeamBoxscores).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
				e.HasOne(x => x.OppTeam).WithMany(y => y.OppTeamBoxscores).HasForeignKey(x => x.OppTeamId).OnDelete(DeleteBehavior.Restrict);
				e.HasOne(x => x.Game).WithMany(y => y.TeamBoxscores).HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Restrict);
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
