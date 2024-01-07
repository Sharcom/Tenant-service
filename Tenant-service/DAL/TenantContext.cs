using DAL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DAL
{
    public class TenantContext: DbContext
    {
        public TenantContext(DbContextOptions options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>()
                .Property(x => x.Members)
                .HasConversion(
                    v => string.Join(',', v.ToArray()),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<List<string>> (
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )                    
                );

            modelBuilder.Entity<Tenant>()
                .Property(x => x.Administrators)
                .HasConversion(
                    v => string.Join(',', v.ToArray()),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
        }
    }
}