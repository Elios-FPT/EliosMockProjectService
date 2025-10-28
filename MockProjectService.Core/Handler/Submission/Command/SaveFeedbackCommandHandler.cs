using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Core.Handler.Submission.Command
{
    public class SaveFeedbackCommandHandler : ICommandHandler<SaveFeedbackCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public SaveFeedbackCommandHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(SaveFeedbackCommand request, CancellationToken cancellationToken)
        {
            if (request.SubmissionId == Guid.Empty)
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "Submission ID cannot be empty.",
                    ResponseData = false
                };
            }

            try
            {
                var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
                if (submission == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Submission not found.",
                        ResponseData = false
                    };
                }

                using var transaction = await _submissionRepository.BeginTransactionAsync();
                try
                {
                    submission.FinalAssessment = request.FinalAssessment;

                    await _submissionRepository.UpdateAsync(submission);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Feedback saved successfully.",
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
                    Message = $"Failed to save feedback: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}