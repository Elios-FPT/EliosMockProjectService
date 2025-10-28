using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Core.Handler.Submission.Command
{
    public class UpdateSubmissionsClassCommandHandler : ICommandHandler<UpdateSubmissionsClassCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.SubmissionsClass> _classRepository;

        public UpdateSubmissionsClassCommandHandler(IGenericRepository<Domain.Entities.SubmissionsClass> classRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(UpdateSubmissionsClassCommand request, CancellationToken cancellationToken)
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
                    entity.Code = request.Code;
                    entity.Status = request.Status;
                    entity.Grade = request.Grade;
                    entity.Assessment = request.Assessment;

                    await _classRepository.UpdateAsync(entity);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Submission class updated successfully.",
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
                    Message = $"Failed to update class: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}