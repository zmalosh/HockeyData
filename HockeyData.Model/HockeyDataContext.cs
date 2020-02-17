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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
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
