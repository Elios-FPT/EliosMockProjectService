using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.TransferObjects
{
    public class SubmissionClassDto
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public Guid SubmissionId { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public double? Grade { get; set; }
        public string Assessment { get; set; }
    }
}
