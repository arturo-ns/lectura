using Microsoft.EntityFrameworkCore;
using pc2_7420_u20231f226.sale.domain.model.agreggates;
using pc2_7420_u20231f226.shared.Persistence.EFC.Extentions;

namespace pc2_7420_u20231f226.sale.infrastructure.persistence.EFC.context;

public class BillContext : DbContext
{
    public DbSet<bill> Bills => Set<bill>();

    public BillContext(DbContextOptions<BillContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply snake case naming convention
        modelBuilder.UserSnakeCaseNamingConventions();
        
        // Configure Bill entity
        modelBuilder.Entity<bill>(entity =>
        {
            // Primary key
            entity.HasKey(b => b.BillNumber);
            
            // Auto-generate bill number
            entity.Property(b => b.BillNumber)
                  .ValueGeneratedOnAdd();

            // Required fields with length constraints
            entity.Property(b => b.Customer)
                  .IsRequired()
                  .HasMaxLength(100);
                  
            entity.Property(b => b.Plate)
                  .HasMaxLength(10);
                  
            entity.Property(b => b.Adviser)
                  .IsRequired()
                  .HasMaxLength(100);
                  
            entity.Property(b => b.Amount)
                  .IsRequired()
                  .HasColumnType("decimal(18,2)");

            // Configure owned type (Invoice)
            entity.OwnsOne(b => b.Invoice, invoice =>
            {
                invoice.Property(i => i.SerialNumber)
                       .IsRequired()
                       .HasMaxLength(10);
                       
                invoice.Property(i => i.SequentialNumber)
                       .IsRequired()
                       .HasMaxLength(10);
            });

            // Configure ServiceId as enum
            entity.Property(b => b.ServiceId)
                  .IsRequired()
                  .HasConversion<int>();

            // Configure audit fields
            entity.Property(b => b.CreatedDate)
                  .IsRequired();
                  
            entity.Property(b => b.UpdatedDate)
                  .IsRequired();

            // Table name in plural
            entity.ToTable("bills");
        });
    }
}