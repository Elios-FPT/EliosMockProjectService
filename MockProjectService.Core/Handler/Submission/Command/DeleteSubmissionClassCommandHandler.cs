using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Core.Handler.Submission.Command
{
    public class DeleteSubmissionsClassCommandHandler : ICommandHandler<DeleteSubmissionsClassCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.SubmissionsClass> _classRepository;

        public DeleteSubmissionsClassCommandHandler(IGenericRepository<Domain.Entities.SubmissionsClass> classRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(DeleteSubmissionsClassCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "Class ID cannot be empty.",
                    ResponseData = false
                };
            }

            try
            {
                var entity = await _classRepository.GetByIdAsync(request.Id);
                if (entity == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Submission class not found.",
                        ResponseData = false
                    };
                }

                using var transaction = await _classRepository.BeginTransactionAsync();
                try
                {
                    await _classRepository.DeleteAsync(entity);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Submission class deleted successfully.",
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
                    Message = $"Failed to delete class: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}