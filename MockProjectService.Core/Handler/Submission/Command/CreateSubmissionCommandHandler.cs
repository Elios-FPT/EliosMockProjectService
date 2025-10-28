using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Core.Handler.Submission.Command
{
    public class CreateSubmissionCommandHandler : ICommandHandler<CreateSubmissionCommand, BaseResponseDto<string>>
    {
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public CreateSubmissionCommandHandler(
            IGenericRepository<Domain.Entities.Submission> submissionRepository,
            IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<string>> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Project ID cannot be empty.",
                    ResponseData = null
                };
            }

            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return new BaseResponseDto<string>
                    {
                        Status = 400,
                        Message = "Mock project not found.",
                        ResponseData = null
                    };
                }

                using var transaction = await _submissionRepository.BeginTransactionAsync();
                try
                {
                    var submission = new Domain.Entities.Submission
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Empty,
                        MockProjectId = request.ProjectId,
                        Status = "Pending",
                        FinalAssessment = ""
                    };

                    await _submissionRepository.AddAsync(submission);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<string>
                    {
                        Status = 200,
                        Message = "Submission created successfully.",
                        ResponseData = submission.Id.ToString()
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
                    Message = $"Failed to create submission: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}