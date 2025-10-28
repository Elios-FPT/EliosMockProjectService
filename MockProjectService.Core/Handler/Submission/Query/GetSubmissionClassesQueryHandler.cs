using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Core.Handler.Submission.Query
{
    public class GetSubmissionClassesQueryHandler : IQueryHandler<GetSubmissionClassesQuery, BaseResponseDto<List<SubmissionClassDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.SubmissionsClass> _classRepository;

        public GetSubmissionClassesQueryHandler(IGenericRepository<Domain.Entities.SubmissionsClass> classRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        public async Task<BaseResponseDto<List<SubmissionClassDto>>> Handle(GetSubmissionClassesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var classes = await _classRepository.GetListAsync();
                var dtos = classes.Select(c => new SubmissionClassDto
                {
                    Id = c.Id,
                    ProcessId = c.ProcessId,
                    SubmissionId = c.SubmissionId,
                    Code = c.Code,
                    Status = c.Status,
                    Grade = c.Grade,
                    Assessment = c.Assessment
                }).ToList();

                return new BaseResponseDto<List<SubmissionClassDto>>
                {
                    Status = 200,
                    Message = "Submission classes retrieved successfully.",
                    ResponseData = dtos
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<List<SubmissionClassDto>>
                {
                    Status = 500,
                    Message = $"Failed to retrieve classes: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}