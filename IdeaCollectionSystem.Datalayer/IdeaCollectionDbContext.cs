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
		// Đã gỡ bỏ DbSet<User> và DbSet<Role> vì chúng được quản lý bởi Identity
		public DbSet<Category> Categories { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Idea> Ideas { get; set; }
		public DbSet<IdeaReaction> IdeaReactions { get; set; }
		public DbSet<EmailOutBox> EmailOutBoxes { get; set; }
		public DbSet<IdeaDocument> IdeaDocuments { get; set; }

		// BỔ SUNG: Các Entities đang có trong thư mục nhưng bị thiếu ở code cũ
		public DbSet<IdeaView> IdeaViews { get; set; }
		public DbSet<TermVersion> TermVersions { get; set; }
		public DbSet<UserTermAcceptance> UserTermAcceptances { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			#region Master Entities
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

				// UserId là string (Identity ID). Đặt MaxLength(450) để khớp chuẩn Identity
				entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);

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
			modelBuilder.Entity<IdeaDocument>(entity =>
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

				entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.Comments)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});
			#endregion

			#region IdeaReactions
			modelBuilder.Entity<IdeaReaction>(entity =>
			{
				entity.ToTable("IdeaReactions");

				// CHUẨN HÓA: Dùng Composite Key để 1 user chỉ được Like/Dislike 1 lần cho 1 ý tưởng
				entity.HasKey(x => new { x.UserId, x.IdeaId });

				entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);

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

			#region Bổ sung các bảng bị thiếu
			modelBuilder.Entity<IdeaView>(entity =>
			{
				entity.ToTable("IdeaViews");
				entity.HasKey(x => x.Id);
			});

			modelBuilder.Entity<TermVersion>(entity =>
			{
				entity.ToTable("TermVersions");
				entity.HasKey(x => x.Id);
			});

			modelBuilder.Entity<UserTermAcceptance>(entity =>
			{
				entity.ToTable("UserTermAcceptances");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);

				// Fix: Use TermId and Term navigation property as per UserTermAcceptance signature
				entity.HasOne(x => x.Term)
					.WithMany()
					.HasForeignKey(x => x.TermId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion
		}
	}
}