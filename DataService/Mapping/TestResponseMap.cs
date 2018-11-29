using DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataService.Mapping
{
    public class TestResponseMap : IEntityTypeConfiguration<TestResponse>
    {
        public void Configure(EntityTypeBuilder<TestResponse> builder)
        {
            builder.ToTable("TestResponse");

            builder.HasKey(c => c.TestResponseId);

            builder.Property(c => c.TestResponseId).HasColumnName("TestResponseId").UseSqlServerIdentityColumn();
            builder.Property(c => c.JobId).HasColumnName("JobId").HasColumnType("int").IsRequired();
            builder.Property(c => c.DateCreated).HasColumnName("DateCreated").HasColumnType("datetime2").IsRequired();
        }
    }
}
