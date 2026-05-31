using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Password).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(u => u.Name, n =>
        {
            n.Property(x => x.Firstname).HasColumnName("Firstname").HasMaxLength(50);
            n.Property(x => x.Lastname).HasColumnName("Lastname").HasMaxLength(50);
        });

        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(x => x.City).HasColumnName("City").HasMaxLength(100);
            a.Property(x => x.Street).HasColumnName("Street").HasMaxLength(200);
            a.Property(x => x.Number).HasColumnName("AddressNumber");
            a.Property(x => x.Zipcode).HasColumnName("Zipcode").HasMaxLength(20);
            a.OwnsOne(x => x.Geolocation, g =>
            {
                g.Property(x => x.Lat).HasColumnName("GeoLat").HasMaxLength(30);
                g.Property(x => x.Long).HasColumnName("GeoLong").HasMaxLength(30);
            });
        });
    }
}
