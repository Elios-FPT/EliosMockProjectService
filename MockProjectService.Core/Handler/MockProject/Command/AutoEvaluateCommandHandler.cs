using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Command;

namespace MockProjectService.Core.Handler.MockProject.Command
{
    public class AutoEvaluateCommandHandler : ICommandHandler<AutoEvaluateCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public AutoEvaluateCommandHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(AutoEvaluateCommand request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty || request.SubmissionId == Guid.Empty)
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "Project ID and Submission ID cannot be empty.",
                    ResponseData = false
                };
            }

            try
            {
                var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
                if (submission == null || submission.MockProjectId != request.ProjectId)
                {
                    return new BaseResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Submission not found or does not belong to the specified project.",
                        ResponseData = false
                    };
                }

                submission.Status = "Evaluated";
                submission.FinalGrade = new Random().Next(50, 100);
                submission.FinalAssessment = "Auto-evaluated by system.";

                using var transaction = await _submissionRepository.BeginTransactionAsync();
                try
                {
                    await _submissionRepository.UpdateAsync(submission);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Auto-evaluation completed successfully.",
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
                    Message = $"Failed to auto-evaluate: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}