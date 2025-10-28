using MockProjectService.Contract.TransferObjects;
using MockProjectService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MockProjectService.Core.Extensions
{
    public static class MappingExtension
    {
        public static MockProjectDto ToDto(this MockProject mockProject)
        {
            var dto = new MockProjectDto
            {
                BaseProjectUrl = mockProject.BaseProjectUrl,
                CreatedAt = mockProject.CreatedAt,
                Description = mockProject.Description,
                Difficulty = mockProject.Difficulty,
                FileName = mockProject.FileName,
                Id = mockProject.Id,
                KeyPrefix = mockProject.KeyPrefix,
                Language = mockProject.Language,
                Title = mockProject.Title,
                UpdateAt = mockProject.UpdateAt
            };
            return dto;
        }

        public static ProcessDto ToDto(this Process process)
        {
            var dto = new ProcessDto
            {
                Id = process.Id,
                BaseClassCode = process.BaseClassCode,
                MockProjectId = process.MockProjectId,
                StepGuiding = process.StepGuiding
            };
            return dto;
        }
    }
}
