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
    public class GetTopSubmissionsQueryHandler : IQueryHandler<GetTopSubmissionsQuery, BaseResponseDto<List<SubmissionDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public GetTopSubmissionsQueryHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<List<SubmissionDto>>> Handle(GetTopSubmissionsQuery request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return new BaseResponseDto<List<SubmissionDto>>
                {
                    Status = 400,
                    Message = "Project ID cannot be empty.",
                    ResponseData = null
                };
            }

            if (request.Top <= 0)
            {
                return new BaseResponseDto<List<SubmissionDto>>
                {
                    Status = 400,
                    Message = "Top must be positive.",
                    ResponseData = null
                };
            }

            try
            {
                var submissions = await _submissionRepository.GetListAsyncUntracked<Domain.Entities.Submission>(
                    filter: s => s.MockProjectId == request.ProjectId && s.FinalGrade.HasValue,
                    orderBy: q => q.OrderByDescending(s => s.FinalGrade),
                    pageSize: request.Top,
                    pageNumber: 1);

                var dtos = submissions.Select(s => s.ToDto()).ToList();

                return new BaseResponseDto<List<SubmissionDto>>
                {
                    Status = 200,
                    Message = dtos.Any() ? "Top submissions retrieved successfully." : "No submissions with grades found.",
                    ResponseData = dtos
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<List<SubmissionDto>>
                {
                    Status = 500,
                    Message = $"Failed to retrieve top submissions: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}