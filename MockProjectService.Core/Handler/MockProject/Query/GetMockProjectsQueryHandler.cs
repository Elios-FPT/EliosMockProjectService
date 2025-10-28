using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Extensions;
using MockProjectService.Core.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Query;

namespace MockProjectService.Core.Handler.MockProject.Query
{
    public class GetMockProjectsQueryHandler : IQueryHandler<GetMockProjectsQuery, BaseResponseDto<BasePagingDto<MockProjectDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public GetMockProjectsQueryHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<BasePagingDto<MockProjectDto>>> Handle(GetMockProjectsQuery request, CancellationToken cancellationToken)
        {
            if (request.Page <= 0 || request.PageSize <= 0)
            {
                return new BaseResponseDto<BasePagingDto<MockProjectDto>>
                {
                    Status = 400,
                    Message = "Page and PageSize must be positive.",
                    ResponseData = null
                };
            }

            try
            {
                var projects = await _projectRepository.GetListAsyncUntracked<Domain.Entities.MockProject>(
                    filter: p =>
                        (string.IsNullOrWhiteSpace(request.Language) || p.Language == request.Language) &&
                        (string.IsNullOrWhiteSpace(request.Difficulty) || p.Difficulty == request.Difficulty),
                    orderBy: q => q.OrderByDescending(p => p.CreatedAt),
                    pageSize: request.PageSize,
                    pageNumber: request.Page);

                var total = await _projectRepository.GetCountAsync(filter: p =>
                        (string.IsNullOrWhiteSpace(request.Language) || p.Language == request.Language) &&
                        (string.IsNullOrWhiteSpace(request.Difficulty) || p.Difficulty == request.Difficulty));

                var dtos = projects.Select(p => p.ToDto());

                var paging = new BasePagingDto<MockProjectDto>
                {
                    PagedData = dtos.ToList(),
                    TotalRecords = total,
                    CurrentPage = request.Page,
                    TotalPages = (int)Math.Ceiling((double)total / request.PageSize),
                    PageSize = request.PageSize
                };

                return new BaseResponseDto<BasePagingDto<MockProjectDto>>
                {
                    Status = 200,
                    Message = projects.Any() ? "Mock projects retrieved successfully." : "No mock projects found.",
                    ResponseData = paging
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<BasePagingDto<MockProjectDto>>
                {
                    Status = 500,
                    Message = $"Failed to retrieve mock projects: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}