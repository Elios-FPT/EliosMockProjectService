using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Extensions;
using MockProjectService.Core.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Core.Handler.Submission.Query
{
    public class GetSubmissionsStatisticsQueryHandler
        : IQueryHandler<GetSubmissionsStatisticsQuery, BaseResponseDto<SubmissionStatisticsDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;
        private readonly IQueryExtensions<Domain.Entities.Submission> _queryExtensions;

        public GetSubmissionsStatisticsQueryHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository, IQueryExtensions<Domain.Entities.Submission> queryExtensions)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _queryExtensions = queryExtensions ?? throw new ArgumentNullException(nameof(queryExtensions));
        }

        public async Task<BaseResponseDto<SubmissionStatisticsDto>> Handle(
            GetSubmissionsStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {

                var submissions = await _submissionRepository.GetListAsync(
                    filter: s =>
                        (!request.UserId.HasValue || s.UserId == request.UserId.Value) &&
                        (string.IsNullOrEmpty(request.Status) || s.Status == request.Status),
                    include: query => _queryExtensions.ApplyIncludes(query, "MockProject")
                );

                if (!submissions.Any())
                {
                    return new BaseResponseDto<SubmissionStatisticsDto>
                    {
                        Status = 200,
                        Message = "No submissions found.",
                        ResponseData = new SubmissionStatisticsDto()
                    };
                }

                var gradedSubmissions = submissions.Where(s => s.FinalGrade.HasValue).ToList();

                var stats = new SubmissionStatisticsDto
                {

                    TotalSubmissions = submissions.Count(),

                    TotalUsers = submissions.Select(s => s.UserId).Distinct().Count(),

                    AverageFinalGrade = gradedSubmissions.Any()
                        ? gradedSubmissions.Average(s => s.FinalGrade.Value)
                        : (double?)null,

                    HighestFinalGrade = gradedSubmissions.Any()
                        ? gradedSubmissions.Max(s => s.FinalGrade.Value)
                        : (double?)null,

                    LowestFinalGrade = gradedSubmissions.Any()
                        ? gradedSubmissions.Min(s => s.FinalGrade.Value)
                        : (double?)null,

                    StatusCounts = submissions
                        .GroupBy(s => s.Status ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),

                    SubmissionsPerProject = submissions
                        .GroupBy(s => s.MockProjectId)
                        .ToDictionary(
                            g => g.First().MockProject?.Id ?? g.Key,
                            g => g.Count()
                        ),

                    TotalPending = submissions.Count(s => s.Status == "Pending"),
                    TotalApproved = submissions.Count(s => s.Status == "Approved"),
                    TotalRejected = submissions.Count(s => s.Status == "Rejected")
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