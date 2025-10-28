using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Domain.Entities
{
    public class MockProject : BaseEntity
    {
        public string Title { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string KeyPrefix { get; set; }
        public string FileName { get; set; }
        public string BaseProjectUrl { get; set; }

        public ICollection<Process> Processes { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}
