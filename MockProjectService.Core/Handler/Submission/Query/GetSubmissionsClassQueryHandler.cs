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
    public class GetSubmissionsClassQueryHandler : IQueryHandler<GetSubmissionsClassQuery, BaseResponseDto<SubmissionsClassDto>>
    {
        private readonly IGenericRepository<Domain.Entities.SubmissionsClass> _classRepository;

        public GetSubmissionsClassQueryHandler(IGenericRepository<Domain.Entities.SubmissionsClass> classRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        public async Task<BaseResponseDto<SubmissionsClassDto>> Handle(GetSubmissionsClassQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                return new BaseResponseDto<SubmissionsClassDto>
                {
                    Status = 400,
                    Message = "Class ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var entity = await _classRepository.GetByIdAsync(request.Id);
                if (entity == null)
                {
                    return new BaseResponseDto<SubmissionsClassDto>
                    {
                        Status = 404,
                        Message = "Submission class not found.",
                        ResponseData = null
                    };
                }

                var dto = new SubmissionsClassDto
                {
                    Id = entity.Id,
                    ProcessId = entity.ProcessId,
                    SubmissionId = entity.SubmissionId,
                    Code = entity.Code,
                    Status = entity.Status,
                    Grade = entity.Grade,
                    Assessment = entity.Assessment
                };

                return new BaseResponseDto<SubmissionsClassDto>
                {
                    Status = 200,
                    Message = "Submission class retrieved successfully.",
                    ResponseData = dto
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<SubmissionsClassDto>
                {
                    Status = 500,
                    Message = $"Failed to retrieve class: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}