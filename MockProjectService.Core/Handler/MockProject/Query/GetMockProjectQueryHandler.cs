using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Extensions;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Query;

namespace MockProjectService.Core.Handler.MockProject.Query
{
    public class GetMockProjectQueryHandler : IQueryHandler<GetMockProjectQuery, BaseResponseDto<MockProjectDto>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public GetMockProjectQueryHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<MockProjectDto>> Handle(GetMockProjectQuery request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return new BaseResponseDto<MockProjectDto>
                {
                    Status = 400,
                    Message = "Project ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return new BaseResponseDto<MockProjectDto>
                    {
                        Status = 404,
                        Message = "Mock project not found.",
                        ResponseData = null
                    };
                }

                return new BaseResponseDto<MockProjectDto>
                {
                    Status = 200,
                    Message = "Mock project retrieved successfully.",
                    ResponseData = project.ToDto()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MockProjectDto>
                {
                    Status = 500,
                    Message = $"Failed to retrieve mock project: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}