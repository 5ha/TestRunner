using DataService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataService.Mapping
{
    public class JobMap : IEntityTypeConfiguration<Job>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Job> builder)
        {
            builder.ToTable("Job");

            builder.HasKey(c => c.JobId);

            builder.Property(c => c.JobId).HasColumnName("JobId").UseSqlServerIdentityColumn();
            builder.Property(c => c.DateCreated).HasColumnName("DateCreated").HasColumnType("datetime2").IsRequired();
            builder.Property(c => c.Description).HasColumnName("Description").HasColumnType("nvarchar(max)").IsRequired();
            builder.Property(c => c.DateCompleted).HasColumnName("DateCompleted").HasColumnType("datetime2");

            builder.HasMany(x => x.TestRequests).WithOne(x => x.Job);
        }
    }
}
