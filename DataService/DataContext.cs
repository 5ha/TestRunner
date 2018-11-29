using DataService.Entities;
using DataService.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DataService
{
    public class DataContext : DbContext
    {
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<TestRequest> TestRequests { get; set; }
        public virtual DbSet<TestResponse> TestResponses { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            :base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new JobMap());
            modelBuilder.ApplyConfiguration(new TestRequestMap());
            modelBuilder.ApplyConfiguration(new TestResponseMap());
        }
    }
}
