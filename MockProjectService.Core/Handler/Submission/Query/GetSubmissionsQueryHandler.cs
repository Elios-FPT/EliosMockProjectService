using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Extensions;
using MockProjectService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Core.Handler.Submission.Query
{
    public class GetSubmissionsQueryHandler : IQueryHandler<GetSubmissionsQuery, BaseResponseDto<List<SubmissionDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;
        private readonly IQueryExtensions<Domain.Entities.Submission> _queryExtensions;

        public GetSubmissionsQueryHandler(
            IGenericRepository<Domain.Entities.Submission> submissionRepository,
            IQueryExtensions<Domain.Entities.Submission> queryExtensions)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _queryExtensions = queryExtensions ?? throw new ArgumentNullException(nameof(queryExtensions));
        }

        public async Task<BaseResponseDto<List<SubmissionDto>>> Handle(GetSubmissionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var submissions = await _submissionRepository.GetListAsync(
                    filter: s =>
                        (!request.UserId.HasValue || s.UserId == request.UserId.Value) &&
                        (!request.ProjectId.HasValue || s.MockProjectId == request.ProjectId.Value) &&
                        (string.IsNullOrEmpty(request.Status) || s.Status == request.Status),
                    include: query => _queryExtensions.ApplyIncludes(query, "MockProject", "SubmissionsClasses")
                );


                var dtos = submissions.Select(s => new SubmissionDto
                {
                    UserId = s.UserId,
                    FinalAssessment = s.FinalAssessment,
                    FinalGrade = s.FinalGrade,
                    Id = s.Id,
                    MockProjectId = s.MockProjectId,
                    Status = s.Status
                }).ToList();

                return new BaseResponseDto<List<SubmissionDto>>
                {
                    Status = 200,
                    Message = "Submissions retrieved successfully.",
                    ResponseData = dtos
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<List<SubmissionDto>>
                {
                    Status = 500,
                    Message = $"Failed to retrieve submissions: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}