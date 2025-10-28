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
    public class GetMockProjectProcessesQueryHandler : IQueryHandler<GetMockProjectProcessesQuery, BaseResponseDto<List<ProcessDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;
        private readonly IQueryExtensions<Domain.Entities.MockProject> _queryExtensions;

        public GetMockProjectProcessesQueryHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository, IQueryExtensions<Domain.Entities.MockProject> queryExtensions)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _queryExtensions = queryExtensions ?? throw new ArgumentNullException(nameof(queryExtensions));
        }

        public async Task<BaseResponseDto<List<ProcessDto>>> Handle(GetMockProjectProcessesQuery request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return new BaseResponseDto<List<ProcessDto>>
                {
                    Status = 400,
                    Message = "Project ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var project = await _projectRepository.GetOneAsyncUntracked<Domain.Entities.MockProject>(
                    filter: p => p.Id == request.ProjectId,
                    include: p => _queryExtensions.ApplyIncludes(p,
                        "Processes"
                    )
                );
                if (project == null)
                {
                    return new BaseResponseDto<List<ProcessDto>>
                    {
                        Status = 404,
                        Message = "Mock project not found.",
                        ResponseData = null
                    };
                }

                var processes = project.Processes?.Select(p => p.ToDto()).ToList() ?? new List<ProcessDto>();

                return new BaseResponseDto<List<ProcessDto>>
                {
                    Status = 200,
                    Message = processes.Any() ? "Processes retrieved successfully." : "No processes found for this project.",
                    ResponseData = processes
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<List<ProcessDto>>
                {
                    Status = 500,
                    Message = $"Failed to retrieve processes: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}