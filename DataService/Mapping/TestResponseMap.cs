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

            builder.HasKey(c => c.TestRequestId);

            builder.Property(c => c.TestRequestId).HasColumnName("TestRequestId").ValueGeneratedNever();
            builder.Property(c => c.DateCreated).HasColumnName("DateCreated").HasColumnType("datetime2").IsRequired();

            builder.HasOne(c => c.TestRequest).WithOne(t => t.TestResponse);
        }
    }
}
