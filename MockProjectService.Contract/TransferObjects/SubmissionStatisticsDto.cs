using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.TransferObjects
{
    public class SubmissionStatisticsDto
    {
        public int TotalSubmissions { get; set; }
        public int TotalUsers { get; set; }
        public double? AverageFinalGrade { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; }
        public Dictionary<int, int> SubmissionsPerProject { get; set; }
        public double? HighestFinalGrade { get; set; }
        public double? LowestFinalGrade { get; set; }
        public int TotalPending { get;set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
    }
}
