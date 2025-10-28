using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Core.Handler.Submission.Query
{
    public class GetSubmissionScoreQueryHandler : IQueryHandler<GetSubmissionScoreQuery, BaseResponseDto<double?>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;
        private readonly IGenericRepository<Domain.Entities.SubmissionsClass> _classRepository;

        public GetSubmissionScoreQueryHandler(
            IGenericRepository<Domain.Entities.Submission> submissionRepository,
            IGenericRepository<Domain.Entities.SubmissionsClass> classRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        public async Task<BaseResponseDto<double?>> Handle(GetSubmissionScoreQuery request, CancellationToken cancellationToken)
        {
            if (request.SubmissionId == Guid.Empty)
            {
                return new BaseResponseDto<double?>
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
                    return new BaseResponseDto<double?>
                    {
                        Status = 404,
                        Message = "Submission not found.",
                        ResponseData = null
                    };
                }

                var classes = await _classRepository.GetListAsync(c => c.SubmissionId == request.SubmissionId);
                var score = classes.Any() ? classes.Average(c => c.Grade ?? 0) : (double?)null;

                return new BaseResponseDto<double?>
                {
                    Status = 200,
                    Message = "Score retrieved successfully.",
                    ResponseData = score
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<double?>
                {
                    Status = 500,
                    Message = $"Failed to get score: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}