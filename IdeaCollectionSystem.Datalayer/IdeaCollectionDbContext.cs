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
		// Bỏ DbSet<User> — không dùng custom Users nữa, dùng AspNetUsers (Identity)
		public DbSet<Category> Categories { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Idea> Ideas { get; set; }
		public DbSet<IdeaReactions> IdeaReactions { get; set; }
		public DbSet<EmailOutBox> EmailOutBoxes { get; set; }
		public DbSet<IdeaDocuments> IdeaDocuments { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			#region Master Entities
			modelBuilder.Entity<Role>(entity =>
			{
				entity.ToTable("Roles");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});

			modelBuilder.Entity<Department>(entity =>
			{
				entity.ToTable("Departments");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});

			modelBuilder.Entity<Category>(entity =>
			{
				entity.ToTable("Categories");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});

			modelBuilder.Entity<Submission>(entity =>
			{
				entity.ToTable("Submissions");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});
			#endregion

			#region Idea
			modelBuilder.Entity<Idea>(entity =>
			{
				entity.ToTable("Ideas");
				entity.HasKey(x => x.Id);

				// UserId là string (Identity ID) — lưu trực tiếp, không FK sang Users
				entity.Property(x => x.UserId).IsRequired();

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

			#region IdeaDocuments
			modelBuilder.Entity<IdeaDocuments>(entity =>
			{
				entity.ToTable("IdeaDocuments");
				entity.HasKey(x => x.Id);

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.IdeaDocuments)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});
			#endregion

			#region Comments
			modelBuilder.Entity<Comment>(entity =>
			{
				entity.ToTable("Comments");
				entity.HasKey(x => x.Id);

				// UserId là string (Identity ID) — lưu trực tiếp, không FK
				entity.Property(x => x.UserId).IsRequired();

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.Comments)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});
			#endregion

			#region IdeaReactions
			modelBuilder.Entity<IdeaReactions>(entity =>
			{
				entity.ToTable("IdeaReactions");
				entity.HasKey(x => x.Id);

				// UserId là string (Identity ID) — lưu trực tiếp, không FK
				entity.Property(x => x.UserId).IsRequired();

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.IdeaReactions)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});
			#endregion

			#region EmailOutBox
			modelBuilder.Entity<EmailOutBox>(entity =>
			{
				entity.ToTable("EmailOutBoxes");
				entity.HasKey(x => x.Id);

				entity.HasOne(x => x.Idea)
					.WithMany()
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});
			#endregion
		}
	}
}