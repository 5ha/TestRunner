using DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace DataService.Mapping
{
    public class TestRequestMap : IEntityTypeConfiguration<TestRequest>
    {
        public void Configure(EntityTypeBuilder<TestRequest> builder)
        {
            builder.ToTable("TestRequest");

            builder.HasKey(c => c.TestRequestId);

            builder.Property(c => c.TestRequestId).HasColumnName("TestRequestId").UseSqlServerIdentityColumn();
            builder.Property(c => c.JobId).HasColumnName("JobId").HasColumnType("int").IsRequired();
            builder.Property(c => c.TestName).HasColumnName("TestName").HasColumnType("nvarchar(max)").IsRequired();
        }
    }
}
