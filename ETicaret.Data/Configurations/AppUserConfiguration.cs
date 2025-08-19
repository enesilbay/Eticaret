using ETicaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETicaret.Data.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Surname).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Email).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Phone).HasColumnType("varchar(15)").HasMaxLength(15);
            builder.Property(x => x.Password).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasColumnType("nvarchar(50)").HasMaxLength(50);
            builder.Property(x => x.UserName).HasColumnType("varchar (50)").HasMaxLength(50);

            builder.HasData(
                new AppUser
                {
                    Id = 1,
                    CreateDate = new DateTime(2024, 1, 1),
                    UserName = "Admin",
                    Email = "admin@gmail.io",
                    IsActive = true,
                    IsAdmin = true,
                    Name = "Test",
                    Password = "123456*",
                    Surname = "User"
                }
                );
        }
    }
}
