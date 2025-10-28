using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Domain.Entities
{
    public class Submission : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid MockProjectId { get; set; }
        public string Status { get; set; }
        public double? FinalGrade { get; set; }
        public string FinalAssessment { get; set; }

        public MockProject MockProject { get; set; }
        public ICollection<SubmissionsClass> SubmissionsClasses { get; set; }
    }
}
