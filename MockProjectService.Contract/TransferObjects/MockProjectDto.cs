using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.TransferObjects
{
    public class MockProjectDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string KeyPrefix { get; set; }
        public string FileName { get; set; }
        public string BaseProjectUrl { get; set; }
    }
}
