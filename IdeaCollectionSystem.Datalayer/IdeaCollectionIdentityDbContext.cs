using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdeaCollectionIdentityDbContext
	: IdentityDbContext<IdeaUser, IdeaRole, string>
{
	public IdeaCollectionIdentityDbContext(
		DbContextOptions<IdeaCollectionIdentityDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<IdeaUser>(entity =>
		{
			entity.Property(n => n.Name)
				  .HasMaxLength(MaxLengths.NAME);

			entity.Property(n => n.Avatar)
				  .HasMaxLength(MaxLengths.FILE_PATH);
		});

		builder.Entity<IdeaRole>(entity =>
		{
			entity.Property(n => n.Description)
				  .HasMaxLength(MaxLengths.DESCRIPTION);
		});
	}
}