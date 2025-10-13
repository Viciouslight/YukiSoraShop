using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Configure Category-Product relationship
        modelBuilder.Entity<Product>(entity =>
        {
            // Primary foreign key relationship using CategoryId
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Categories_CategoryId");
                
            // Configure CategoryName as display field
            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(255);
                
            // Create index for CategoryName for better search performance
            entity.HasIndex(e => e.CategoryName)
                .HasDatabaseName("IX_Products_CategoryName");
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Categories");
            
            entity.HasIndex(e => e.CategoryName)
                .IsUnique()
                .HasDatabaseName("IX_Categories_CategoryName_Unique");
            
            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
                
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
        });
    }
}
