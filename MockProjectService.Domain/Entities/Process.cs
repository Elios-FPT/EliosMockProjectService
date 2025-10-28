using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Domain.Entities
{
    public class Process : BaseEntity
    {
        public Guid MockProjectId { get; set; }
        public string BaseClassCode { get; set; }
        public string StepGuiding { get; set; }

        public MockProject MockProject { get; set; }
        public ICollection<SubmissionsClass> SubmissionsClasses { get; set; }
    }
}
