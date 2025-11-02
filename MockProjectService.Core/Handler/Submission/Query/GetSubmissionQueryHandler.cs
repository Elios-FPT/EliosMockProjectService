using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Core.Handler.Submission.Query
{
    public class GetSubmissionQueryHandler : IQueryHandler<GetSubmissionQuery, BaseResponseDto<SubmissionDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public GetSubmissionQueryHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<SubmissionDto>> Handle(GetSubmissionQuery request, CancellationToken cancellationToken)
        {
            if (request.SubmissionId == Guid.Empty)
            {
                return new BaseResponseDto<SubmissionDto>
                {
                    Status = 400,
                    Message = "Submission ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
                if (submission == null)
                {
                    return new BaseResponseDto<SubmissionDto>
                    {
                        Status = 404,
                        Message = "Submission not found.",
                        ResponseData = null
                    };
                }

                var dto = new SubmissionDto
                {
                    UserId = submission.UserId,
                    FinalAssessment = submission.FinalAssessment,
                    FinalGrade = submission.FinalGrade,
                    Id = submission.Id,
                    MockProjectId = submission.MockProjectId,
                    Status = submission.Status
                };

                return new BaseResponseDto<SubmissionDto>
                {
                    Status = 200,
                    Message = "Submission retrieved successfully.",
                    ResponseData = dto
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<SubmissionDto>
                {
                    Status = 500,
                    Message = $"Failed to retrieve submission: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}