using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Process.Command;

namespace MockProjectService.Core.Handler.Process.Command
{
    public class UpdateProcessCommandHandler : ICommandHandler<UpdateProcessCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Process> _processRepository;

        public UpdateProcessCommandHandler(IGenericRepository<Domain.Entities.Process> processRepository)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(UpdateProcessCommand request, CancellationToken cancellationToken)
        {
            if (request.ProcessId == Guid.Empty)
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "Process ID cannot be empty.",
                    ResponseData = false
                };
            }

            if (string.IsNullOrWhiteSpace(request.StepGuiding))
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "StepGuiding cannot be null or empty.",
                    ResponseData = false
                };
            }

            try
            {
                var process = await _processRepository.GetByIdAsync(request.ProcessId);
                if (process == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Process not found.",
                        ResponseData = false
                    };
                }

                using var transaction = await _processRepository.BeginTransactionAsync();
                try
                {
                    process.StepGuiding = request.StepGuiding;
                    process.BaseClassCode = request.BaseClassCode;

                    await _processRepository.UpdateAsync(process);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Process updated successfully.",
                        ResponseData = true
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<bool>
                {
                    Status = 500,
                    Message = $"Failed to update process: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}