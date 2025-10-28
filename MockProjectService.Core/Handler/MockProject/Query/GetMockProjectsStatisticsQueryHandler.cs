using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Extensions;
using MockProjectService.Core.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Query;

namespace MockProjectService.Core.Handler.MockProject.Query
{
    public class GetMockProjectsStatisticsQueryHandler : IQueryHandler<GetMockProjectsStatisticsQuery, BaseResponseDto<MockProjectStatisticsDto>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;
        private readonly IQueryExtensions<Domain.Entities.MockProject> _queryExtensions;
        public GetMockProjectsStatisticsQueryHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<MockProjectStatisticsDto>> Handle(GetMockProjectsStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var projects = await _projectRepository.GetListAsync(include: p => _queryExtensions.ApplyIncludes(p, "Submissions"));

                var totalProjects = projects.Count();
                var totalSubmissions = projects.Sum(p => p.Submissions?.Count ?? 0);
                var totalProcesses = projects.Sum(p => p.Processes?.Count ?? 0);

                var stats = new MockProjectStatisticsDto
                {
                    TotalProjects = totalProjects,
                    LanguageCounts = projects.GroupBy(p => p.Language).ToDictionary(g => g.Key, g => g.Count()),
                    DifficultyCounts = projects.GroupBy(p => p.Difficulty).ToDictionary(g => g.Key, g => g.Count()),
                    AverageSubmissionsPerProject = totalProjects > 0 ? (double)totalSubmissions / totalProjects : 0,
                    AverageProcessesPerProject = totalProjects > 0 ? (double)totalProcesses / totalProjects : 0,
                    TotalSubmissions = totalSubmissions,
                    TotalProcesses = totalProcesses,
                    OldestProjectCreatedAt = projects.Any() ? projects.Min(p => p.CreatedAt) : null,
                    LatestProjectCreatedAt = projects.Any() ? projects.Max(p => p.CreatedAt) : null,
                    ActiveProjects = projects.Count()
                };

                return new BaseResponseDto<MockProjectStatisticsDto>
                {
                    Status = 200,
                    Message = "Statistics retrieved successfully.",
                    ResponseData = stats
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MockProjectStatisticsDto>
                {
                    Status = 500,
                    Message = $"Failed to retrieve statistics: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}