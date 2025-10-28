using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.TransferObjects
{
    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MockProjectId { get; set; }
        public string Status { get; set; }
        public double? FinalGrade { get; set; }
        public string FinalAssessment { get; set; }
    }
}
