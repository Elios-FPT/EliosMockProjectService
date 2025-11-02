using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Process.Query;

namespace MockProjectService.Core.Handler.Process.Query
{
    public class GetProcessQueryHandler : IQueryHandler<GetProcessQuery, BaseResponseDto<ProcessDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Process> _processRepository;

        public GetProcessQueryHandler(IGenericRepository<Domain.Entities.Process> processRepository)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
        }

        public async Task<BaseResponseDto<ProcessDto>> Handle(GetProcessQuery request, CancellationToken cancellationToken)
        {
            if (request.ProcessId == Guid.Empty)
            {
                return new BaseResponseDto<ProcessDto>
                {
                    Status = 400,
                    Message = "Process ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var process = await _processRepository.GetByIdAsync(request.ProcessId);
                if (process == null)
                {
                    return new BaseResponseDto<ProcessDto>
                    {
                        Status = 404,
                        Message = "Process not found.",
                        ResponseData = null
                    };
                }

                var processDto = new ProcessDto
                {
                    Id = process.Id,
                    MockProjectId = process.MockProjectId,
                    StepNumber = process.StepNumber,
                    StepGuiding = process.StepGuiding,
                    BaseClassCode = process.BaseClassCode,
                };

                return new BaseResponseDto<ProcessDto>
                {
                    Status = 200,
                    Message = "Process retrieved successfully.",
                    ResponseData = processDto
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProcessDto>
                {
                    Status = 500,
                    Message = $"Failed to retrieve process: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}