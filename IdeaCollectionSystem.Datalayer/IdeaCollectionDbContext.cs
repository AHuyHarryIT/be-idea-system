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

		#region DbSet
		public DbSet<User> Users { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Idea> Ideas { get; set; }
		public DbSet<EmailOutBox> EmailOutBoxes { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			#region User
			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("Users");

				entity.Property(x => x.UserName)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();

				entity.Property(x => x.FirstName)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();

				entity.Property(x => x.LastName)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();

				entity.Property(x => x.HashPassword)
					  .HasMaxLength(MaxLengths.HASH_PASSWORD)
					  .IsRequired();

				entity.Property(x => x.Avartar)
					  .HasMaxLength(MaxLengths.FILE_PATH)
					  .IsRequired();
			});
			#endregion

			#region Category
			modelBuilder.Entity<Category>(entity =>
			{
				entity.ToTable("Categories");

				entity.Property(x => x.Name)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();
			});
			#endregion

			#region Department
			modelBuilder.Entity<Department>(entity =>
			{
				entity.ToTable("Departments");

				entity.Property(x => x.Name)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();

				entity.Property(x => x.Description)
					  .HasMaxLength(MaxLengths.DESCRIPTION);
			});
			#endregion

			#region Role
			modelBuilder.Entity<Role>(entity =>
			{
				entity.ToTable("Roles");

				entity.Property(x => x.Name)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();
			});
			#endregion

			#region Submission
			modelBuilder.Entity<Submission>(entity =>
			{
				entity.ToTable("Submissions");

				entity.Property(x => x.Name)
					  .HasMaxLength(MaxLengths.NAME)
					  .IsRequired();
			});
			#endregion

			#region Idea
			modelBuilder.Entity<Idea>(entity =>
			{
				entity.ToTable("Ideas");

				entity.Property(x => x.Text)
					  .HasMaxLength(MaxLengths.TEXT)
					  .IsRequired();

				
				entity.HasOne(x => x.User)
					  .WithMany(x => x.Ideas)
					  .HasForeignKey(x => x.UserId)
					  .OnDelete(DeleteBehavior.Restrict);
			});
			#endregion

			#region Comment
			modelBuilder.Entity<Comment>(entity =>
			{
				entity.ToTable("Comments");

				entity.Property(x => x.Text)
					  .HasMaxLength(MaxLengths.COMMENT)
					  .IsRequired();

				entity.HasOne(x => x.User)
					  .WithMany(x => x.Comments)
					  .HasForeignKey(x => x.UserId)
					  .OnDelete(DeleteBehavior.Restrict);

				
				entity.HasOne(x => x.Idea)
					  .WithMany(x => x.Comments)
					  .HasForeignKey(x => x.IdeaId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
			#endregion

			modelBuilder.Entity<EmailOutBox>(entity =>
			{
				entity.HasOne(e => e.Idea)
					  .WithMany()
					  .HasForeignKey(e => e.IdeaId)
					  .OnDelete(DeleteBehavior.Restrict); 
			});

		}
	}
}
