using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.TransferObjects
{
    public class MockProjectStatisticsDto
    {
        public int TotalProjects { get; set; }
        public Dictionary<string, int> LanguageCounts { get; set; }
        public Dictionary<string, int> DifficultyCounts { get; set; }
        public double AverageSubmissionsPerProject { get; set; }
        public double AverageProcessesPerProject { get; set; }
        public int TotalSubmissions { get; set; }
        public int TotalProcesses { get; set; }
        public DateTime? OldestProjectCreatedAt { get; set; }
        public DateTime? LatestProjectCreatedAt { get; set; }
        public int ActiveProjects { get; set; }
    }
}
