using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.TransferObjects
{
    public class ProcessDto
    {
        public Guid Id { get; set; }
        public Guid MockProjectId { get; set; }
        public string BaseClassCode { get; set; }
        public string StepGuiding { get; set; }
        public int StepNumber { get; set; }
    }
}
