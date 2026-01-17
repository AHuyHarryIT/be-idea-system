
using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Datalayer
{
	public class IdeaCollectionDbContext : DbContext
	{
		public IdeaCollectionDbContext(DbContextOptions<IdeaCollectionDbContext> options)
			: base(options)
		{
		}


		public DbSet<User> Users { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Idea> Ideas { get; set; }
		public DbSet<EmailOutBox> EmailOutBoxes { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);


			// User
			modelBuilder.Entity<User>()
				.Property(s => s.UserName)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			modelBuilder.Entity<User>()
				.Property(s => s.FirstName)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			modelBuilder.Entity<User>()
				.Property(s => s.LastName)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			modelBuilder.Entity<User>()
				.Property(s => s.HashPassword)
				.HasMaxLength(MaxLengths.HASH_PASSWORD)
				.IsRequired();

			modelBuilder.Entity<User>()
				.Property(s => s.Avartar)
				.HasMaxLength(MaxLengths.FILE_PATH)
				.IsRequired();

			// Category
			modelBuilder.Entity<Category>()
				.Property(s => s.Name)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			// Department
			modelBuilder.Entity<Department>()
				.Property(s => s.Name)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			modelBuilder.Entity<Department>()
				.Property(s => s.Description)
				.HasMaxLength(MaxLengths.DESCRIPTION);

			// Role
			modelBuilder.Entity<Role>()
				.Property(s => s.Name)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			modelBuilder.Entity<Department>()
				.Property(s => s.Description)
				.HasMaxLength(MaxLengths.DESCRIPTION);

			// Submission
			modelBuilder.Entity<Submission>()
				.Property(s => s.Name)
				.HasMaxLength(MaxLengths.NAME)
				.IsRequired();

			// Idea
			modelBuilder.Entity<Idea>()
				.Property(s => s.Text)
				.HasMaxLength(MaxLengths.TEXT)
				.IsRequired();

			// Comment
			modelBuilder.Entity<Comment>()
				.Property(s => s.Text)
				.HasMaxLength(MaxLengths.COMMENT)
				.IsRequired();

			// EmailOutBox
			modelBuilder.Entity<EmailOutBox>()
				.Property(s => s.EmailTo)
				.HasMaxLength(MaxLengths.EMAIL_ADDRESS)
				.IsRequired();

			modelBuilder.Entity<EmailOutBox>()
				.Property(s => s.Subject)
				.HasMaxLength(MaxLengths.TITLE)
				.IsRequired();

			modelBuilder.Entity<EmailOutBox>()
				.Property(s => s.Body)
				.HasMaxLength(MaxLengths.TEXT)
				.IsRequired();

			modelBuilder.Entity<EmailOutBox>()
				.Property(s => s.Error)
				.HasMaxLength(MaxLengths.TEXT);

		}
	}
}