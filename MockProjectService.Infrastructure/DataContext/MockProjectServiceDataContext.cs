using Microsoft.EntityFrameworkCore;
using MockProjectService.Domain.Entities;

namespace MockProjectService.Infrastructure.DataContext
{
    public class MockProjectServiceDataContext : DbContext
    {
        public MockProjectServiceDataContext(DbContextOptions<MockProjectServiceDataContext> options) : base(options)
        {
        }

        public virtual DbSet<MockProject> MockProjects { get; set; }

        public virtual DbSet<Process> Processes { get; set; }

        public virtual DbSet<Submission> Submissions { get; set; }

        public virtual DbSet<SubmissionsClass> SubmissionsClasses { get; set; }
    }
}
