using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdeaCollectionIdentityDbContext
	: IdentityDbContext<IdeaUser, IdeaRole, string>
{
	public IdeaCollectionIdentityDbContext(DbContextOptions<IdeaCollectionIdentityDbContext> options)
		: base(options)
	{
	}

	// Đảm bảo dùng số nhiều cho nhất quán
	public DbSet<IdeaUser> Users { get; set; }
	public DbSet<Department> Departments { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<Department>().ToTable("Departments");

		builder.Entity<IdeaUser>(entity =>
		{
			entity.ToTable("AspNetUsers"); 

			entity.Property(n => n.Name)
				  .HasMaxLength(MaxLengths.NAME);

			entity.Property(n => n.DepartmentId)
				  .IsRequired(false);

	
			entity.HasOne(u => u.Department)
				  .WithMany()
				  .HasForeignKey(u => u.DepartmentId)
				  .OnDelete(DeleteBehavior.SetNull);
		});

		builder.Entity<IdeaRole>(entity =>
		{
			entity.Property(n => n.Description)
				  .HasMaxLength(MaxLengths.DESCRIPTION);
		});
	}
}