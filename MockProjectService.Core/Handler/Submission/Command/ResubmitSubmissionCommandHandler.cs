using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Core.Handler.Submission.Command
{
    public class ResubmitSubmissionCommandHandler : ICommandHandler<ResubmitSubmissionCommand, BaseResponseDto<string>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public ResubmitSubmissionCommandHandler(IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<string>> Handle(ResubmitSubmissionCommand request, CancellationToken cancellationToken)
        {
            if (request.SubmissionId == Guid.Empty)
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Submission ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var original = await _submissionRepository.GetByIdAsync(request.SubmissionId);
                if (original == null)
                {
                    return new BaseResponseDto<string>
                    {
                        Status = 404,
                        Message = "Submission not found.",
                        ResponseData = null
                    };
                }

                using var transaction = await _submissionRepository.BeginTransactionAsync();
                try
                {
                    var newSubmission = new Domain.Entities.Submission
                    {
                        Id = Guid.NewGuid(),
                        UserId = original.UserId,
                        MockProjectId = original.MockProjectId,
                        Status = "Pending",
                    };

                    await _submissionRepository.AddAsync(newSubmission);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<string>
                    {
                        Status = 200,
                        Message = "Submission resubmitted successfully.",
                        ResponseData = newSubmission.Id.ToString()
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
                return new BaseResponseDto<string>
                {
                    Status = 500,
                    Message = $"Failed to resubmit: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}