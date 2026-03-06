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
		public DbSet<IdeaDocuments> IdeaDocuments { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			#region Master Entities
			modelBuilder.Entity<Role>(entity => {
				entity.ToTable("Roles");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});

			modelBuilder.Entity<Department>(entity => {
				entity.ToTable("Departments");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});

			modelBuilder.Entity<Category>(entity => {
				entity.ToTable("Categories");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});

			modelBuilder.Entity<Submission>(entity => {
				entity.ToTable("Submissions");
				entity.HasKey(x => x.Id);
				entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
			});
			#endregion

			#region User Entity
			modelBuilder.Entity<User>(entity => {
				entity.ToTable("Users");
				entity.HasKey(x => x.Id);

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

			#region Idea Entity
			modelBuilder.Entity<Idea>(entity => {
				entity.ToTable("Ideas");
				entity.HasKey(x => x.Id);

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

			#region IdeaReactions (Fixed Shadow Properties)
			modelBuilder.Entity<IdeaReactions>(entity => {
				entity.ToTable("IdeaReactions");
				
				entity.HasKey(x => new { x.UserId, x.IdeaId });

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.IdeaReactions)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.User)
					.WithMany() 
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion

			#region IdeaDocuments
			modelBuilder.Entity<IdeaDocuments>(entity => {
				entity.ToTable("IdeaDocuments");
				entity.HasKey(x => x.Id);

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.IdeaDocuments)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);
			});
			#endregion

			#region Comment
			modelBuilder.Entity<Comment>(entity => {
				entity.ToTable("Comments");

				entity.HasOne(x => x.Idea)
					.WithMany(x => x.Comments)
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.User)
					.WithMany(x => x.Comments)
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion

			#region EmailOutBox
			modelBuilder.Entity<EmailOutBox>(entity => {
				entity.ToTable("EmailOutBoxes");

				entity.HasOne(x => x.Idea)
					.WithMany() 
					.HasForeignKey(x => x.IdeaId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(x => x.Comment)
					.WithMany()
					.HasForeignKey(x => x.CommentId)
					.OnDelete(DeleteBehavior.Restrict);
			});
			#endregion
		}
	}
}