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
		public DbSet<IdeaReactions> IdeaReactions { get; set; }
		public DbSet<EmailOutBox> EmailOutBoxes { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			#region Master Entities: Role, Department, Category, Submission
			
			modelBuilder.Entity<Role>(entity =>
			{
				entity.ToTable("Roles");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(MaxLengths.NAME).IsRequired();
			});

			modelBuilder.Entity<Department>(entity =>
			{
				entity.ToTable("Departments");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(MaxLengths.NAME).IsRequired();
				entity.Property(x => x.Description).HasMaxLength(MaxLengths.DESCRIPTION);
			});

			modelBuilder.Entity<Category>(entity =>
			{
				entity.ToTable("Categories");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(MaxLengths.NAME).IsRequired();
			});

			modelBuilder.Entity<Submission>(entity =>
			{
				entity.ToTable("Submissions");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(MaxLengths.NAME).IsRequired();
			
				entity.Property(x => x.ClousureDate).IsRequired();
				entity.Property(x => x.FinalClousureDate).IsRequired();
			});
			#endregion

			#region User (Security Entity)
			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("Users");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.UserName).HasMaxLength(MaxLengths.NAME).IsRequired();
				entity.Property(x => x.HashPassword).HasMaxLength(MaxLengths.HASH_PASSWORD).IsRequired();

			
				entity.HasOne(x => x.Role)
					.WithMany(x => x.Users)
					.HasForeignKey(x => x.RoleId)
					.OnDelete(DeleteBehavior.Restrict);

			
				entity.HasOne(x => x.Department)
					.WithMany(x => x.Users)
					.HasForeignKey(x => x.DepartmentId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion

			#region Idea (Core Transaction Entity)
			modelBuilder.Entity<Idea>(entity =>
			{
				entity.ToTable("Ideas");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Text).HasMaxLength(MaxLengths.TEXT).IsRequired();

				
				entity.HasOne(x => x.User)
					.WithMany(x => x.Ideas)
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Restrict);

				
				entity.HasOne(x => x.Submission)
					.WithMany(x => x.Ideas)
					.HasForeignKey(x => x.SubmissionId)
					.OnDelete(DeleteBehavior.Restrict);

				
				entity.HasOne(x => x.Category)
					.WithMany(x => x.Ideas)
					.HasForeignKey(x => x.CategoryId)
					.OnDelete(DeleteBehavior.Restrict);

				
				entity.HasOne(x => x.Department)
					.WithMany(x => x.Ideas)
					.HasForeignKey(x => x.DepartmentId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion

			#region IdeaReactions (Fixed Logic)
			modelBuilder.Entity<IdeaReactions>(entity =>
			{
				entity.ToTable("IdeaReactions");

				// Khóa chính phức hợp
				entity.HasKey(x => new { x.UserId, x.IdeaId });

				// Quan hệ với Idea
				entity.HasOne(x => x.Idea)
					.WithMany(x => x.IdeaReactions)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.User)
					.WithMany() 
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.Property(x => x.Reaction).IsRequired();
			});
			#endregion


			#region Comment & Email Log
			modelBuilder.Entity<Comment>(entity =>
			{
				entity.ToTable("Comments");
				entity.Property(x => x.Text).HasMaxLength(MaxLengths.COMMENT).IsRequired();

				entity.HasOne(x => x.User)
					.WithMany(x => x.Comments)
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.Comments)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<EmailOutBox>(entity =>
			{
				entity.ToTable("EmailOutBoxes");
			
				entity.HasOne(e => e.Comment)
					.WithMany()
					.HasForeignKey(e => e.CommentId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(e => e.Idea)
					.WithMany()
					.HasForeignKey(e => e.IdeaId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion
		}
	}
}