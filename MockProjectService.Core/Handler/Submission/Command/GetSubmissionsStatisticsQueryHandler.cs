using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Core.Handler.Submission.Query
{
    public class GetSubmissionsStatisticsQueryHandler : IQueryHandler<GetSubmissionsStatisticsQuery, BaseResponseDto<SubmissionStatisticsDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public GetSubmissionsStatisticsQueryHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<SubmissionStatisticsDto>> Handle(GetSubmissionsStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var submissions = await _submissionRepository.GetListAsync(
                    filter: s =>
                        (!request.UserId.HasValue || s.UserId == request.UserId.Value) &&
                        (string.IsNullOrEmpty(request.Status) || s.Status == request.Status)
                );

                var stats = new SubmissionStatisticsDto
                {
                    TotalSubmissions = submissions.Count(),
                    StatusCounts = submissions.GroupBy(s => s.Status).ToDictionary(g => g.Key, g => g.Count()),
                    AverageFinalGrade = submissions.Any(s => s.FinalGrade.HasValue) ? submissions.Where(s => s.FinalGrade.HasValue).Average(s => s.FinalGrade.Value) : 0,
                    TotalApproved = submissions.Count() > 0 ? 1 : 0
                };

                return new BaseResponseDto<SubmissionStatisticsDto>
                {
                    Status = 200,
                    Message = "Statistics retrieved successfully.",
                    ResponseData = stats
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<SubmissionStatisticsDto>
                {
                    Status = 500,
                    Message = $"Failed to retrieve statistics: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}